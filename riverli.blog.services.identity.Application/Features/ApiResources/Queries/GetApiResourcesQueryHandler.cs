using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using riverli.blog.services.identity.Application.Constants;
using riverli.blog.services.identity.Application.DTOs.ApiResources;
using riverli.blog.services.identity.Domain.Entities;
using RiverLi.DDD.Core.Domain.Repositories;

namespace riverli.blog.services.identity.Application.Features.ApiResources.Queries;

public class GetApiResourcesQueryHandler : IRequestHandler<GetApiResourcesQuery, List<ApiResourceDto>>
{
    private readonly IRepository<ApiResource, Guid> _repository;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;

    public GetApiResourcesQueryHandler(IRepository<ApiResource, Guid> repository, IMemoryCache cache, IConfiguration configuration)
    {
        _repository = repository;
        _cache = cache;
        _configuration = configuration;
    }

    public async Task<List<ApiResourceDto>> Handle(GetApiResourcesQuery request, CancellationToken cancellationToken)
    {
        if (!_cache.TryGetValue(CacheKeys.SysApiResources, out List<ApiResourceDto>? apiResources))
        {
            var all = await _repository.AsQueryable()
                .OrderBy(a => a.ServiceName)
                .ThenBy(a => a.Route)
                .ToListAsync(cancellationToken);

            apiResources = all.Select(a => new ApiResourceDto
            {
                Id = a.Id,
                ServiceName = a.ServiceName,
                Method = a.Method,
                Route = a.Route,
                Description = a.Description,
                IsPublic = a.IsPublic
            }).ToList();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(12));

            _cache.Set(CacheKeys.SysApiResources, apiResources, cacheOptions);
        }
        return apiResources!;
    }
}
