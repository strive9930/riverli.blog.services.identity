using MediatR;
using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Menus.Commands;

/// <summary>
/// 获取菜单详情查询处理器
/// </summary>
public class GetMenuByIdQueryHandler : IRequestHandler<GetMenuByIdQuery, Result<MenuDto>>
{
    private readonly IdentityServiceDbContext _context;

    public GetMenuByIdQueryHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<MenuDto>> Handle(GetMenuByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var menu = await _context.Menus
                .Include(m => m.Children)
                .Include(m => m.Parent)
                .Include(m => m.FrontendRoute)
                .Include(m => m.MenuGroup)
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (menu == null)
            {
                return Result<MenuDto>.FailResult("菜单不存在");
            }

            var dto = MapToDto(menu);
            return Result<MenuDto>.SuccessResult(dto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching menu {request.Id}: {ex.Message}");
            return Result<MenuDto>.FailResult("获取菜单失败");
        }
    }

    private MenuDto MapToDto(Menu menu)
    {
        return new MenuDto
        {
            Id = menu.Id,
            Name = menu.Name,
            Title = menu.Title,
            Path = menu.Path,
            Icon = menu.Icon,
            Sort = menu.Sort,
            IsEnabled = menu.IsEnabled,
            IsVisible = menu.IsVisible,
            ParentId = menu.ParentId,
            MenuType = (int)menu.MenuType,
            FrontendRouteId = menu.FrontendRouteId,
            MenuGroupId = menu.MenuGroupId,
            RequiredPermission = menu.RequiredPermission,
            Description = menu.Description,
            Meta = menu.Meta,
            CreateTime = menu.CreateTime,
            UpdateTime = menu.UpdateTime,
            Parent = menu.Parent != null ? MapToDto(menu.Parent) : null,
            Children = menu.Children.Select(MapToDto).ToList(),
            MenuGroup = menu.MenuGroup != null ? new MenuGroupDto
            {
                Id = menu.MenuGroup.Id,
                Name = menu.MenuGroup.Name,
                Code = menu.MenuGroup.Code
            } : null,
            FrontendRoutePath = menu.FrontendRoute?.Path
        };
    }
}

/// <summary>
/// 获取所有菜单查询处理器
/// </summary>
public class GetAllMenusQueryHandler : IRequestHandler<GetAllMenusQuery, Result<List<MenuDto>>>
{
    private readonly IdentityServiceDbContext _context;

    public GetAllMenusQueryHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<MenuDto>>> Handle(GetAllMenusQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.Menus
                .Include(m => m.Children)
                .Include(m => m.Parent)
                .Include(m => m.FrontendRoute)
                .Include(m => m.MenuGroup)
                .AsQueryable();

            // 根据菜单组筛选
            if (request.MenuGroupId.HasValue)
            {
                query = query.Where(m => m.MenuGroupId == request.MenuGroupId.Value);
            }

            // 根据菜单类型筛选
            if (request.MenuType.HasValue)
            {
                query = query.Where(m => (int)m.MenuType == request.MenuType.Value);
            }

            // 根据启用状态筛选
            if (request.IsEnabled.HasValue)
            {
                query = query.Where(m => m.IsEnabled == request.IsEnabled.Value);
            }

            // 根据可见性筛选
            if (request.IsVisible.HasValue)
            {
                query = query.Where(m => m.IsVisible == request.IsVisible.Value);
            }

            // 根据关键词筛选
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(m => 
                    m.Name.Contains(request.Keyword) || 
                    m.Title.Contains(request.Keyword) ||
                    (m.Description != null && m.Description.Contains(request.Keyword)));
            }

            var menus = await query
                .OrderBy(m => m.Sort)
                .ThenBy(m => m.Name)
                .ToListAsync(cancellationToken);

            var dtos = menus.Select(menu => new MenuDto
            {
                Id = menu.Id,
                Name = menu.Name,
                Title = menu.Title,
                Path = menu.Path,
                Icon = menu.Icon,
                Sort = menu.Sort,
                IsEnabled = menu.IsEnabled,
                IsVisible = menu.IsVisible,
                ParentId = menu.ParentId,
                MenuType = (int)menu.MenuType,
                FrontendRouteId = menu.FrontendRouteId,
                MenuGroupId = menu.MenuGroupId,
                RequiredPermission = menu.RequiredPermission,
                Description = menu.Description,
                Meta = menu.Meta,
                CreateTime = menu.CreateTime,
                UpdateTime = menu.UpdateTime,
                Parent = menu.Parent != null ? new MenuDto
                {
                    Id = menu.Parent.Id,
                    Name = menu.Parent.Name,
                    Title = menu.Parent.Title
                } : null,
                Children = menu.Children.Select(child => new MenuDto
                {
                    Id = child.Id,
                    Name = child.Name,
                    Title = child.Title
                }).ToList(),
                MenuGroup = menu.MenuGroup != null ? new MenuGroupDto
                {
                    Id = menu.MenuGroup.Id,
                    Name = menu.MenuGroup.Name,
                    Code = menu.MenuGroup.Code
                } : null,
            }).ToList();

            return Result<List<MenuDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching menus: {ex.Message}");
            return Result<List<MenuDto>>.FailResult("获取菜单列表失败");
        }
    }
}

/// <summary>
/// 分页获取菜单查询处理器
/// </summary>
public class GetPagedMenusQueryHandler : IRequestHandler<GetPagedMenusQuery, PagedResult<MenuDto>>
{
    private readonly IdentityServiceDbContext _context;

    public GetPagedMenusQueryHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<MenuDto>> Handle(GetPagedMenusQuery request, CancellationToken cancellationToken)
    {
        try
        {
            Console.WriteLine($"[GetPagedMenus] 开始查询 - PageIndex: {request.PageIndex}, PageSize: {request.PageSize}, Keyword: '{request.Keyword}'");
            
            var query = _context.Menus
                .Include(m => m.Parent)
                .Include(m => m.Children)
                .Include(m => m.FrontendRoute)
                .Include(m => m.MenuGroup)
                .AsQueryable();

            // 根据菜单组筛选
            if (request.MenuGroupId.HasValue)
            {
                query = query.Where(m => m.MenuGroupId == request.MenuGroupId.Value);
            }

            // 根据菜单类型筛选
            if (request.MenuType.HasValue)
            {
                query = query.Where(m => (int)m.MenuType == request.MenuType.Value);
            }

            // 根据启用状态筛选
            if (request.IsEnabled.HasValue)
            {
                query = query.Where(m => m.IsEnabled == request.IsEnabled.Value);
            }

            // 根据可见性筛选
            if (request.IsVisible.HasValue)
            {
                query = query.Where(m => m.IsVisible == request.IsVisible.Value);
            }

            // 根据关键词筛选
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                query = query.Where(m => 
                    m.Name.Contains(request.Keyword) || 
                    m.Title.Contains(request.Keyword) ||
                    (m.Description != null && m.Description.Contains(request.Keyword)));
            }

            // 排序
            query = request.SortBy.ToLower() switch
            {
                "name" => request.SortDesc ? query.OrderByDescending(m => m.Name) : query.OrderBy(m => m.Name),
                "title" => request.SortDesc ? query.OrderByDescending(m => m.Title) : query.OrderBy(m => m.Title),
                "sort" => request.SortDesc ? query.OrderByDescending(m => m.Sort) : query.OrderBy(m => m.Sort),
                "createtime" => request.SortDesc ? query.OrderByDescending(m => m.CreateTime) : query.OrderBy(m => m.CreateTime),
                _ => request.SortDesc ? query.OrderByDescending(m => m.Sort) : query.OrderBy(m => m.Sort)
            };

            // 分页
            var totalCount = await query.CountAsync(cancellationToken);
            Console.WriteLine($"[GetPagedMenus] 总记录数: {totalCount}");
            
            var menus = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);
            
            Console.WriteLine($"[GetPagedMenus] 当前页返回记录数: {menus.Count}");

            var dtos = menus.Select(menu => new MenuDto
            {
                Id = menu.Id,
                Name = menu.Name,
                Title = menu.Title,
                Path = menu.Path,
                Icon = menu.Icon,
                Sort = menu.Sort,
                IsEnabled = menu.IsEnabled,
                IsVisible = menu.IsVisible,
                ParentId = menu.ParentId,
                MenuType = (int)menu.MenuType,
                FrontendRouteId = menu.FrontendRouteId,
                MenuGroupId = menu.MenuGroupId,
                RequiredPermission = menu.RequiredPermission,
                Description = menu.Description,
                Meta = menu.Meta,
                CreateTime = menu.CreateTime,
                UpdateTime = menu.UpdateTime,
                // 父菜单信息
                Parent = menu.Parent != null ? new MenuDto
                {
                    Id = menu.Parent.Id,
                    Name = menu.Parent.Name,
                    Title = menu.Parent.Title,
                    Path = menu.Parent.Path
                } : null,
                // 子菜单列表
                Children = menu.Children.Select(child => new MenuDto
                {
                    Id = child.Id,
                    Name = child.Name,
                    Title = child.Title,
                    Path = child.Path,
                    Icon = child.Icon,
                    Sort = child.Sort,
                    ParentId = child.ParentId
                }).ToList(),
                MenuGroup = menu.MenuGroup != null ? new MenuGroupDto
                {
                    Id = menu.MenuGroup.Id,
                    Name = menu.MenuGroup.Name,
                    Code = menu.MenuGroup.Code
                } : null,
                FrontendRoutePath = menu.FrontendRoute?.Path
            }).ToList();

            return new PagedResult<MenuDto>
            {
                Data = dtos,
                TotalCount = totalCount,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching paged menus: {ex.Message}");
            throw;
        }
    }
}

/// <summary>
/// 获取菜单树结构查询处理器
/// </summary>
public class GetMenuTreeQueryHandler : IRequestHandler<GetMenuTreeQuery, Result<List<MenuTreeNodeDto>>>
{
    private readonly IdentityServiceDbContext _context;

    public GetMenuTreeQueryHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<MenuTreeNodeDto>>> Handle(GetMenuTreeQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.Menus
                .AsNoTracking()  // 只读查询,使用 NoTracking 提升性能
                .AsQueryable();

            // 根据菜单组筛选
            if (request.MenuGroupId.HasValue)
            {
                query = query.Where(m => m.MenuGroupId == request.MenuGroupId.Value);
            }

            // 根据菜单类型筛选
            if (request.MenuType.HasValue)
            {
                query = query.Where(m => (int)m.MenuType == request.MenuType.Value);
            }

            // 根据启用状态筛选
            if (request.IsEnabled.HasValue)
            {
                query = query.Where(m => m.IsEnabled == request.IsEnabled.Value);
            }

            // 根据可见性筛选
            if (request.IsVisible.HasValue)
            {
                query = query.Where(m => m.IsVisible == request.IsVisible.Value);
            }

            // 获取所有符合条件的菜单
            var allMenus = await query
                .OrderBy(m => m.Sort)
                .ThenBy(m => m.Name)
                .ToListAsync(cancellationToken);

            Console.WriteLine($"[GetMenuTree] Total menus loaded: {allMenus.Count}");
            Console.WriteLine($"[GetMenuTree] Root menus (no parent): {allMenus.Count(m => !m.ParentId.HasValue)}");

            // 如果用户权限为空，返回所有菜单
            if (request.UserPermissions == null || !request.UserPermissions.Any())
            {
                var rootMenus = allMenus.Where(m => !m.ParentId.HasValue).ToList();
                var treeNodes = BuildTree(rootMenus, allMenus);
                Console.WriteLine($"[GetMenuTree] Tree nodes built: {treeNodes.Count}");
                return Result<List<MenuTreeNodeDto>>.SuccessResult(treeNodes);
            }

            // 根据用户权限过滤菜单
            var filteredMenus = FilterMenusByPermissions(allMenus, request.UserPermissions);
            Console.WriteLine($"[GetMenuTree] Filtered menus by permissions: {filteredMenus.Count}");
            var rootFilteredMenus = filteredMenus.Where(m => !m.ParentId.HasValue).ToList();
            var filteredTreeNodes = BuildTree(rootFilteredMenus, filteredMenus);
            
            return Result<List<MenuTreeNodeDto>>.SuccessResult(filteredTreeNodes);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching menu tree: {ex.Message}");
            return Result<List<MenuTreeNodeDto>>.FailResult("获取菜单树失败");
        }
    }

    private List<MenuTreeNodeDto> BuildTree(List<Menu> rootMenus, List<Menu> allMenus)
    {
        var nodes = new List<MenuTreeNodeDto>();
        
        foreach (var menu in rootMenus)
        {
            var node = new MenuTreeNodeDto
            {
                Id = menu.Id,
                Name = menu.Name,
                Title = menu.Title,
                Path = menu.Path,
                Icon = menu.Icon,
                Sort = menu.Sort,
                IsEnabled = menu.IsEnabled,
                IsVisible = menu.IsVisible,
                MenuType = (int)menu.MenuType,
                ParentId = menu.ParentId,
                Children = new List<MenuTreeNodeDto>()  // 初始化空列表
            };

            // 递归构建子节点
            var children = allMenus.Where(m => m.ParentId == menu.Id).ToList();
            if (children.Any())
            {
                node.Children = BuildTree(children, allMenus);
            }

            nodes.Add(node);
        }

        return nodes;
    }

    private List<Menu> FilterMenusByPermissions(List<Menu> menus, List<string> userPermissions)
    {
        return menus.Where(menu => 
            string.IsNullOrEmpty(menu.RequiredPermission) || 
            userPermissions.Contains(menu.RequiredPermission)).ToList();
    }
}