using MediatR;
using Microsoft.Extensions.Caching.Memory;
using riverli.blog.services.identity.Application.Constants;
using riverli.blog.services.identity.Application.DTOs;
using riverli.blog.services.identity.Application.Utils;
using riverli.blog.services.identity.Domain.Entities;
using RiverLi.DDD.Core.Domain.Repositories;

namespace riverli.blog.services.identity.Application.Features.Menus.Queries;

/// <summary>
/// 获取完整菜单树（递归构建父子层级结构）
/// </summary>
public class GetMenuTreeQueryHandler : IRequestHandler<GetMenuTreeQuery, List<MenuTreeDto>>
{
    private readonly IRepository<Menu, Guid> _repository;
    private readonly IMemoryCache _cache;

    public GetMenuTreeQueryHandler(IRepository<Menu, Guid> repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<List<MenuTreeDto>> Handle(GetMenuTreeQuery request, CancellationToken cancellationToken)
    {
        if (!_cache.TryGetValue(CacheKeys.SysMenuTree, out List<MenuTreeDto>? menuTree))
        {
            var allMenus = await _repository.GetAllAsync(cancellationToken);
            menuTree = TreeBuilder.BuildMenuTree(allMenus);

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(12));

            _cache.Set(CacheKeys.SysMenuTree, menuTree, cacheOptions);
        }
        return menuTree;
    }
}
