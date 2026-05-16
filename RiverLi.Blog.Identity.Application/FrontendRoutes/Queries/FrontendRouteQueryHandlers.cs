using MediatR;
using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.FrontendRoutes.Commands;

/// <summary>
/// 获取前端路由详情查询处理器
/// </summary>
public class GetFrontendRouteByIdQueryHandler : IRequestHandler<GetFrontendRouteByIdQuery, Result<FrontendRoute>>
{
    private readonly IdentityServiceDbContext _context;

    public GetFrontendRouteByIdQueryHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<FrontendRoute>> Handle(GetFrontendRouteByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var route = await _context.FrontendRoutes
                .Include(r => r.Children)
                .Include(r => r.BackendRoutes)
                .Include(r => r.RouteGroup)
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (route == null)
            {
                return Result<FrontendRoute>.FailResult("前端路由不存在");
            }

            return Result<FrontendRoute>.SuccessResult(route);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching frontend route {request.Id}: {ex.Message}");
            return Result<FrontendRoute>.FailResult("获取前端路由失败");
        }
    }
}

/// <summary>
/// 获取所有前端路由查询处理器
/// </summary>
public class GetAllFrontendRoutesQueryHandler : IRequestHandler<GetAllFrontendRoutesQuery, Result<List<FrontendRoute>>>
{
    private readonly IdentityServiceDbContext _context;

    public GetAllFrontendRoutesQueryHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<FrontendRoute>>> Handle(GetAllFrontendRoutesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var routes = await _context.FrontendRoutes
                .Include(r => r.Children)
                .Include(r => r.RouteGroup)
                .OrderBy(r => r.Sort)
                .ThenBy(r => r.Path)
                .ToListAsync(cancellationToken);

            return Result<List<FrontendRoute>>.SuccessResult(routes);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching all frontend routes: {ex.Message}");
            return Result<List<FrontendRoute>>.FailResult("获取前端路由列表失败");
        }
    }
}

/// <summary>
/// 获取启用的前端路由查询处理器
/// </summary>
public class GetEnabledFrontendRoutesQueryHandler : IRequestHandler<GetEnabledFrontendRoutesQuery, Result<List<FrontendRoute>>>
{
    private readonly IdentityServiceDbContext _context;

    public GetEnabledFrontendRoutesQueryHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<FrontendRoute>>> Handle(GetEnabledFrontendRoutesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var routes = await _context.FrontendRoutes
                .Include(r => r.RouteGroup)
                .Where(r => r.IsEnabled)
                .OrderBy(r => r.Sort)
                .ThenBy(r => r.Path)
                .ToListAsync(cancellationToken);

            return Result<List<FrontendRoute>>.SuccessResult(routes);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching enabled frontend routes: {ex.Message}");
            return Result<List<FrontendRoute>>.FailResult("获取启用的前端路由失败");
        }
    }
}

/// <summary>
/// 分页获取前端路由查询处理器
/// </summary>
public class GetPagedFrontendRoutesQueryHandler : IRequestHandler<GetPagedFrontendRoutesQuery, PagedResult<FrontendRoute>>
{
    private readonly IdentityServiceDbContext _context;

    public GetPagedFrontendRoutesQueryHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<FrontendRoute>> Handle(GetPagedFrontendRoutesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.FrontendRoutes
                .Include(r => r.RouteGroup)
                .AsQueryable();

            // 关键字搜索
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.ToLower();
                query = query.Where(r =>
                    r.Path.ToLower().Contains(keyword) ||
                    r.Name.ToLower().Contains(keyword) ||
                    (r.Title != null && r.Title.ToLower().Contains(keyword)) ||
                    (r.Component != null && r.Component.ToLower().Contains(keyword)));
            }

            // 状态筛选
            if (request.IsEnabled.HasValue)
            {
                query = query.Where(r => r.IsEnabled == request.IsEnabled.Value);
            }

            // 路由组筛选
            if (request.RouteGroupId.HasValue)
            {
                query = query.Where(r => r.RouteGroupId == request.RouteGroupId.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var routes = await query
                .OrderBy(r => r.Sort)
                .ThenBy(r => r.Path)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return PagedResult<FrontendRoute>.SuccessResult(routes, totalCount, request.PageIndex, request.PageSize);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching paged frontend routes: {ex.Message}");
            return PagedResult<FrontendRoute>.FailResult($"获取前端路由列表失败：{ex.Message}");
        }
    }
}

/// <summary>
/// 获取菜单树结构查询处理器
/// </summary>
public class GetMenuTreeQueryHandler : IRequestHandler<GetMenuTreeQuery, Result<List<FrontendRoute>>>
{
    private readonly IdentityServiceDbContext _context;

    public GetMenuTreeQueryHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<FrontendRoute>>> Handle(GetMenuTreeQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // 获取所有启用且显示在菜单中的路由
            var allRoutes = await _context.FrontendRoutes
                .Include(r => r.Children)
                .Include(r => r.RouteGroup)
                .Where(r => r.IsEnabled && r.IsMenu)
                .OrderBy(r => r.Sort)
                .ToListAsync(cancellationToken);

            // 如果用户权限为空，返回所有菜单
            if (request.UserPermissions == null || !request.UserPermissions.Any())
            {
                return Result<List<FrontendRoute>>.SuccessResult(BuildMenuTree(allRoutes));
            }

            // 根据用户权限过滤菜单
            var filteredRoutes = FilterRoutesByPermissions(allRoutes, request.UserPermissions);
            var menuTree = BuildMenuTree(filteredRoutes);

            return Result<List<FrontendRoute>>.SuccessResult(menuTree);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching menu tree: {ex.Message}");
            return Result<List<FrontendRoute>>.FailResult("获取菜单树失败");
        }
    }

    /// <summary>
    /// 根据权限过滤路由
    /// </summary>
    private List<FrontendRoute> FilterRoutesByPermissions(List<FrontendRoute> routes, List<string> permissions)
    {
        return routes.Where(route =>
        {
            // 如果路由不需要权限或用户拥有该权限，则保留
            return string.IsNullOrEmpty(route.RequiredPermission) ||
                   permissions.Contains(route.RequiredPermission);
        }).ToList();
    }

    /// <summary>
    /// 构建菜单树结构
    /// </summary>
    private List<FrontendRoute> BuildMenuTree(List<FrontendRoute> routes)
    {
        var menuTree = new List<FrontendRoute>();
        var routeDict = routes.ToDictionary(r => r.Id, r => r);

        foreach (var route in routes)
        {
            if (!route.ParentId.HasValue)
            {
                // 顶级菜单
                menuTree.Add(route);
            }
            else
            {
                // 子菜单，添加到父菜单的Children集合中
                if (routeDict.TryGetValue(route.ParentId.Value, out var parent))
                {
                    parent.Children.Add(route);
                }
            }
        }

        return menuTree;
    }
}