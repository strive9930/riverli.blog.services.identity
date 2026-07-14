using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.OpenApi.Readers;
using riverli.blog.services.identity.Application.Constants;
using riverli.blog.services.identity.Application.Interfaces;
using riverli.blog.services.identity.Domain.Entities;
using RiverLi.DDD.Core.Domain.Repositories;

namespace riverli.blog.services.identity.Application.Features.ApiResources.Commands;

public class SyncApisCommandHandler : IRequestHandler<SyncApisCommand, bool>
{
    private readonly IRepository<ApiResource, Guid> _apiRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IMemoryCache _cache;
    private readonly IHttpClientFactory _httpClientFactory;

    public SyncApisCommandHandler(
        IHttpClientFactory httpClientFactory,
        IRepository<ApiResource, Guid> apiRepository,
        IRoleRepository roleRepository,
        IMemoryCache cache)
    {
        _httpClientFactory = httpClientFactory;
        _apiRepository = apiRepository;
        _roleRepository = roleRepository;
        _cache = cache;
    }

    public async Task<bool> Handle(SyncApisCommand request, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
        using var stream = await client.GetStreamAsync(request.SwaggerUrl, cancellationToken);

        var openApiDoc = new OpenApiStreamReader().Read(stream, out var diagnostic);
        if (openApiDoc == null) throw new InvalidOperationException("解析 OpenAPI 文档失败");

        var parsedApis = new List<ApiResource>();
        foreach (var path in openApiDoc.Paths)
        {
            foreach (var operation in path.Value.Operations)
            {
                var method = operation.Key.ToString().ToUpper();
                var route = path.Key.StartsWith("/") ? path.Key : "/" + path.Key;
                var description = operation.Value.Summary ?? operation.Value.Description ?? $"{method} {route}";
                var isPublic = operation.Value.Extensions.ContainsKey("x-allow-anonymous");

                parsedApis.Add(new ApiResource
                {
                    ServiceName = request.ServiceName,
                    Method = method,
                    Route = route,
                    Description = description,
                    IsPublic = isPublic
                });
            }
        }

        return await SaveApisToDatabaseAsync(request.ServiceName, parsedApis, cancellationToken);
    }

    // 核心入库对比逻辑 (与 Report 复用相同思想)
    private async Task<bool> SaveApisToDatabaseAsync(string serviceName, List<ApiResource> newApis,
        CancellationToken cancellationToken)
    {
        var existingApis = await _apiRepository.AsQueryable()
            .Where(a => a.ServiceName == serviceName)
            .ToListAsync(cancellationToken);

        foreach (var parsed in newApis)
        {
            var existing = existingApis.FirstOrDefault(a => a.Method == parsed.Method && a.Route == parsed.Route);
            if (existing != null)
            {
                existing.Description = parsed.Description;
                existing.IsPublic = parsed.IsPublic;
                await _apiRepository.UpdateAsync(existing, cancellationToken);
                existingApis.Remove(existing);
            }
            else
            {
                await _apiRepository.AddAsync(parsed, cancellationToken);
            }
        }

        // 清除废弃接口及角色关联
        if (existingApis.Any())
        {
            var oldIds = existingApis.Select(a => a.Id).ToList();
            await _roleRepository.DeleteRoleApisByApiIdsAsync(oldIds, cancellationToken);

            foreach (var api in existingApis)
            {
                await _apiRepository.DeleteAsync(api, cancellationToken);
            }
        }

        await _apiRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        _cache.Remove(CacheKeys.SysApiResources);
        _cache.Remove(CacheKeys.SysApiTree);

        return true;
    }
}
