using MediatR;
using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Users.Commands.User;

/// <summary>
/// 用户前端数据查询处理器
/// </summary>
public class UserFrontendQueryHandlers :
    IRequestHandler<GetUserInfoQuery, Result<UserInfoDto>>,
    IRequestHandler<GetUserMenusQuery, Result<List<MenuTreeDto>>>,
    IRequestHandler<GetUserPermissionCodesQuery, Result<List<string>>>,
    IRequestHandler<GetCurrentUserRolesQuery, Result<List<RoleDto>>>,
    IRequestHandler<GetDashboardStatsQuery, Result<DashboardStatsDto>>
{
    private readonly IdentityServiceDbContext _context;

    public UserFrontendQueryHandlers(IdentityServiceDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 获取用户信息
    /// </summary>
    public async Task<Result<UserInfoDto>> Handle(GetUserInfoQuery request, CancellationToken cancellationToken)
    {
        try
        {
            Console.WriteLine($"UserFrontendQueryHandlers.Handle - UserId: {request.UserId}");
            var user = await _context.Users.FindAsync(request.UserId);
            Console.WriteLine($"UserFrontendQueryHandlers.Handle - User found: {user != null}");
            if (user == null)
            {
                return Result<UserInfoDto>.FailResult("用户不存在");
            }

            var roles = await _context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r)
                .Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Code = r.Code,
                    Description = r.Description
                })
                .ToListAsync(cancellationToken);

            var userInfo = new UserInfoDto
            {
                Id = user.Id.ToString(),
                Username = user.UserName ?? string.Empty,
                Email = user.Email,
                Nickname = user.NickName,
                AvatarUrl = user.AvatarUrl,
                PhoneNumber = user.PhoneNumber,
                CreateTime = user.CreateTime,
                Roles = roles
            };

            return Result<UserInfoDto>.SuccessResult(userInfo);
        }
        catch (Exception ex)
        {
            return Result<UserInfoDto>.FailResult($"获取用户信息失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 获取用户菜单（树形结构）
    /// </summary>
    public async Task<Result<List<MenuTreeDto>>> Handle(GetUserMenusQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // 获取用户的所有权限码
            var permissionCodes = await GetUserPermissionCodes(request.UserId, cancellationToken);

            // 获取用户的所有角色
            var userRoles = await _context.UserRoles
                .Where(ur => ur.UserId == request.UserId)
                .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r)
                .ToListAsync(cancellationToken);

            bool isAdmin = userRoles.Any(r => r.Name == "Admin" || r.Name == "Administrator");

            // 获取所有启用的菜单
            var allMenus = await _context.Menus
                .Where(m => m.IsEnabled && m.IsVisible)
                .OrderBy(m => m.Sort)
                .ToListAsync(cancellationToken);

            // 根据权限和角色过滤菜单
            var filteredMenus = allMenus.Where(m =>
            {
                // 如果没有 RequiredPermission，所有人都可以访问
                if (string.IsNullOrEmpty(m.RequiredPermission))
                {
                    return true;
                }

                // 如果是后台管理菜单（通过路径判断），只有管理员可以访问
                bool isAdminMenu = IsAdminMenu(m);
                if (isAdminMenu && !isAdmin)
                {
                    return false;
                }
                
                // 检查是否有对应权限
                return permissionCodes.Contains(m.RequiredPermission);
            }).ToList();

            // 构建树形结构
            var menuTree = BuildMenuTree(filteredMenus, null);

            return Result<List<MenuTreeDto>>.SuccessResult(menuTree);
        }
        catch (Exception ex)
        {
            return Result<List<MenuTreeDto>>.FailResult($"获取菜单失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 获取用户权限码列表
    /// </summary>
    public async Task<Result<List<string>>> Handle(GetUserPermissionCodesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var permissionCodes = await GetUserPermissionCodes(request.UserId, cancellationToken);
            return Result<List<string>>.SuccessResult(permissionCodes.ToList());
        }
        catch (Exception ex)
        {
            return Result<List<string>>.FailResult($"获取权限失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 获取当前用户角色列表
    /// </summary>
    public async Task<Result<List<RoleDto>>> Handle(GetCurrentUserRolesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var roles = await _context.UserRoles
                .Where(ur => ur.UserId == request.UserId)
                .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r)
                .Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Code = r.Code,
                    Description = r.Description
                })
                .ToListAsync(cancellationToken);

            return Result<List<RoleDto>>.SuccessResult(roles);
        }
        catch (Exception ex)
        {
            return Result<List<RoleDto>>.FailResult($"获取角色失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 获取仪表盘统计数据
    /// </summary>
    public async Task<Result<DashboardStatsDto>> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var stats = new DashboardStatsDto
            {
                TotalUsers = await _context.Users.CountAsync(cancellationToken),
                TotalArticles = 0, // 需要从文章服务获取
                TotalVisits = 0,   // 需要从访问统计服务获取
                RecentActivities = new List<object>()
            };

            return Result<DashboardStatsDto>.SuccessResult(stats);
        }
        catch (Exception ex)
        {
            return Result<DashboardStatsDto>.FailResult($"获取统计数据失败：{ex.Message}");
        }
    }

    #region Helper Methods

    /// <summary>
    /// 获取用户的权限码列表
    /// </summary>
    private async Task<HashSet<string>> GetUserPermissionCodes(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            return new HashSet<string>();
        }

        var permissionCodes = new HashSet<string>();

        // 2. 获取用户直接拥有的权限（通过 UserPermissions 表）
        var userPermissions = await _context.UserPermissions
            .Where(up => up.UserId == userId)
            .Select(up => up.Permission)
            .ToListAsync(cancellationToken);

        foreach (var permission in userPermissions)
        {
            if (!string.IsNullOrEmpty(permission.Code))
            {
                permissionCodes.Add(permission.Code);
            }
        }
        Console.WriteLine($"用户 {userId} 直接权限：{string.Join(", ", permissionCodes)}");

        // 3. 获取用户的所有角色 ID（通过 AspNetUserRoles 表）
        var userRoleIds = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync(cancellationToken);

        // 4. 获取每个角色的权限
        foreach (var roleId in userRoleIds)
        {
            var role = await _context.Roles
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

            if (role != null)
            {
                Console.WriteLine($"检查角色：{role.Name ?? "Unknown"}, 权限数量：{role.Permissions.Count}");
                foreach (var permission in role.Permissions)
                {
                    if (!string.IsNullOrEmpty(permission.Code))
                    {
                        permissionCodes.Add(permission.Code);
                        Console.WriteLine($"添加角色权限：{permission.Code}");
                    }
                }
            }
        }

        Console.WriteLine($"用户 {userId} 总权限：{string.Join(", ", permissionCodes)}");
        return permissionCodes;
    }

    /// <summary>
    /// 构建菜单树形结构
    /// </summary>
    private List<MenuTreeDto> BuildMenuTree(List<Menu> menus, Guid? parentId)
    {
        var parentMenus = menus.Where(m => m.ParentId == parentId).ToList();
        var result = new List<MenuTreeDto>();

        foreach (var menu in parentMenus)
        {
            var menuItem = new MenuTreeDto
            {
                Id = menu.Id.ToString(),
                Name = menu.Name,
                Title = menu.Title,
                Path = menu.Path,
                Icon = menu.Icon,
                Sort = menu.Sort,
                MenuType = menu.MenuType.ToString(),
                RequiredPermission = menu.RequiredPermission,
                Children = BuildMenuTree(menus, menu.Id)
            };

            result.Add(menuItem);
        }

        return result;
    }

    /// <summary>
    /// 判断是否为后台管理菜单
    /// 根据路径前缀判断：/admin/、/system/、/management/ 等开头的路径视为后台管理菜单
    /// </summary>
    private bool IsAdminMenu(Menu menu)
    {
        // 使用 TargetAudience 字段判断，而不是硬编码路径
        return menu.TargetAudience == MenuTarget.Admin;
    }

    #endregion
}
