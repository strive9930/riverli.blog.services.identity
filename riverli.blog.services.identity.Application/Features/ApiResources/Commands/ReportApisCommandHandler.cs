using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using riverli.blog.services.identity.Application.Constants;
using riverli.blog.services.identity.Application.Interfaces;
using riverli.blog.services.identity.Domain.Entities;
using RiverLi.DDD.Core.Domain.Repositories;

namespace riverli.blog.services.identity.Application.Features.ApiResources.Commands;

public class ReportApisCommandHandler : IRequestHandler<ReportApisCommand, bool>
{
    private readonly IRepository<ApiResource, Guid> _apiRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IMemoryCache _cache;

    public ReportApisCommandHandler(
        IMemoryCache cache,
        IRepository<ApiResource, Guid> apiRepository,
        IRoleRepository roleRepository)
    {
        _cache = cache;
        _apiRepository = apiRepository;
        _roleRepository = roleRepository;
    }

    public async Task<bool> Handle(ReportApisCommand request, CancellationToken cancellationToken)
    {
        var existingApis = await _apiRepository.AsQueryable()
            .Where(a => a.ServiceName == request.ServiceName)
            .ToListAsync(cancellationToken);

        foreach (var reported in request.Apis)
        {
            var existing = existingApis.FirstOrDefault(a => a.Method == reported.Method && a.Route == reported.Route);
            if (existing != null)
            {
                existing.Description = reported.Description;
                existing.IsPublic = reported.IsPublic;
                await _apiRepository.UpdateAsync(existing, cancellationToken);
                existingApis.Remove(existing);
            }
            else
            {
                await _apiRepository.AddAsync(new ApiResource
                {
                    ServiceName = request.ServiceName,
                    Method = reported.Method,
                    Route = reported.Route,
                    Description = reported.Description,
                    IsPublic = reported.IsPublic
                }, cancellationToken);
            }
        }

        // 清除废弃接口及角色关联
        if (existingApis.Any())
        {
            var oldIds = existingApis.Select(x => x.Id).ToList();
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
