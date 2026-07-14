using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using riverli.blog.services.identity.Application.Constants;
using riverli.blog.services.identity.Application.DTOs.ApiResources;
using riverli.blog.services.identity.Domain.Entities;
using RiverLi.DDD.Core.Domain.Repositories;

namespace riverli.blog.services.identity.Application.Features.ApiResources.Queries;

public class GetApiResourceTreeQueryHandler : IRequestHandler<GetApiResourceTreeQuery, List<ApiResourceTreeNodeDto>>
{
    private readonly IRepository<ApiResource, Guid> _repository;
    private readonly IMemoryCache _cache;

    public GetApiResourceTreeQueryHandler(IMemoryCache cache, IRepository<ApiResource, Guid> repository)
    {
        _cache = cache;
        _repository = repository;
    }

    public async Task<List<ApiResourceTreeNodeDto>> Handle(GetApiResourceTreeQuery request, CancellationToken cancellationToken)
    {
        if (!_cache.TryGetValue(CacheKeys.SysApiTree, out List<ApiResourceTreeNodeDto>? tree))
        {
            // 获取数据库中所有的 API 资源记录
            var allApis = await _repository.AsQueryable()
                .OrderBy(a => a.ServiceName)
                .ThenBy(a => a.Route)
                .ToListAsync(cancellationToken);

            // 使用 LINQ 按微服务名称 (ServiceName) 进行分组
            tree = allApis.GroupBy(a => a.ServiceName)
                .Select(group => new ApiResourceTreeNodeDto
                {
                    // 核心对接点：给分组的 ID 加上 "group_" 前缀！
                    // 这完美呼应了在 Vue 中写的过滤逻辑：!id.startsWith('group_')
                    Id = $"group_{group.Key}",

                    Label = string.IsNullOrEmpty(group.Key) ? "未分类接口" : $"{group.Key} 微服务",

                    // 将该分组下的 API 映射为子节点
                    Children = group.Select(api => new ApiResourceTreeNodeDto
                    {
                        Id = api.Id.ToString(),
                        Label = string.IsNullOrEmpty(api.Description)
                                ? api.Route
                                : $"{api.Route} ({api.Description})",
                        Method = api.Method.ToUpper()
                    }).ToList()
                }).ToList();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(12));

            _cache.Set(CacheKeys.SysApiTree, tree, cacheOptions);
        }
        return tree!;
    }
}