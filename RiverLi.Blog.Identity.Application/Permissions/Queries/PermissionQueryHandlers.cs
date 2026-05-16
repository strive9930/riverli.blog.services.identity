using MediatR;
using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Application.Permissions.Commands;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Permissions.Queries;

/// <summary>
/// 获取分组权限查询处理器
/// </summary>
public class GetGroupedPermissionsQueryHandler : IRequestHandler<GetGroupedPermissionsQuery, Result<List<GroupedPermissionsDto>>>
{
    private readonly IdentityServiceDbContext _context;

    public GetGroupedPermissionsQueryHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<GroupedPermissionsDto>>> Handle(GetGroupedPermissionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.Permissions.AsQueryable();

            // 如果有搜索关键字，进行过滤
            if (!string.IsNullOrWhiteSpace(request.SearchKeyword))
            {
                var keyword = request.SearchKeyword.ToLower();
                query = query.Where(p => 
                    p.Name.ToLower().Contains(keyword) || 
                    p.Code.ToLower().Contains(keyword) ||
                    p.Group.ToLower().Contains(keyword));
            }

            var permissions = await query
                .OrderBy(p => p.Group)
                .ThenBy(p => p.Name)
                .ToListAsync(cancellationToken);

            // 按分组组织权限
            var groupedPermissions = permissions
                .GroupBy(p => p.Group)
                .Select(g => new GroupedPermissionsDto
                {
                    GroupName = g.Key,
                    Permissions = g.Select(p => new PermissionDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Code = p.Code,
                        Group = p.Group,
                    }).ToList()
                })
                .ToList();

            return Result<List<GroupedPermissionsDto>>.SuccessResult(groupedPermissions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching grouped permissions: {ex.Message}");
            return Result<List<GroupedPermissionsDto>>.FailResult("获取分组权限失败");
        }
    }
}

/// <summary>
/// 获取角色权限状态查询处理器
/// </summary>
public class GetRolePermissionStatusQueryHandler : IRequestHandler<GetRolePermissionStatusQuery, Result<RolePermissionStatusDto>>
{
    private readonly IdentityServiceDbContext _context;

    public GetRolePermissionStatusQueryHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<RolePermissionStatusDto>> Handle(GetRolePermissionStatusQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var role = await _context.Roles
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

            if (role == null)
            {
                return Result<RolePermissionStatusDto>.FailResult("角色不存在");
            }

            var allPermissions = await _context.Permissions.ToListAsync(cancellationToken);
            var assignedPermissionIds = role.Permissions.Select(p => p.Id).ToList();

            // 计算分配给该角色的用户数量
            var userCount = await _context.UserRoles
                .CountAsync(ur => ur.RoleId == role.Id, cancellationToken);

            var result = new RolePermissionStatusDto
            {
                RoleId = role.Id,
                RoleName = role.Name ?? string.Empty,
                AssignedPermissionIds = assignedPermissionIds,
                UserCount = userCount,
                PermissionCount = allPermissions.Count,
                AssignedPermissions = assignedPermissionIds.Count
            };

            return Result<RolePermissionStatusDto>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching role permission status: {ex.Message}");
            return Result<RolePermissionStatusDto>.FailResult("获取角色权限状态失败");
        }
    }
}

/// <summary>
/// 搜索权限查询处理器
/// </summary>
public class SearchPermissionsQueryHandler : IRequestHandler<SearchPermissionsQuery, Result<List<PermissionSearchResultDto>>>
{
    private readonly IdentityServiceDbContext _context;

    public SearchPermissionsQueryHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<PermissionSearchResultDto>>> Handle(SearchPermissionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var keyword = request.Keyword.ToLower();
            
            var permissions = await _context.Permissions
                .Where(p => 
                    p.Name.ToLower().Contains(keyword) || 
                    p.Code.ToLower().Contains(keyword) ||
                    p.Group.ToLower().Contains(keyword))
                .Take(request.PageSize)
                .Select(p => new PermissionSearchResultDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Code = p.Code,
                    Group = p.Group,
                    RelevanceScore = CalculateRelevanceScore(p, keyword)
                })
                .OrderByDescending(p => p.RelevanceScore)
                .ToListAsync(cancellationToken);

            return Result<List<PermissionSearchResultDto>>.SuccessResult(permissions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching permissions: {ex.Message}");
            return Result<List<PermissionSearchResultDto>>.FailResult("搜索权限失败");
        }
    }

    /// <summary>
    /// 计算权限与搜索关键词的相关性评分
    /// </summary>
    private double CalculateRelevanceScore(Permission permission, string keyword)
    {
        double score = 0;
        
        // 名称完全匹配得分最高
        if (permission.Name.ToLower().Equals(keyword))
            score += 10;
        else if (permission.Name.ToLower().Contains(keyword))
            score += 5;
            
        // 代码匹配
        if (permission.Code.ToLower().Equals(keyword))
            score += 8;
        else if (permission.Code.ToLower().Contains(keyword))
            score += 4;
            
        // 分组匹配
        if (permission.Group.ToLower().Equals(keyword))
            score += 3;
        else if (permission.Group.ToLower().Contains(keyword))
            score += 1;
            
        return score;
    }
}

/// <summary>
/// 获取权限统计信息查询处理器
/// </summary>
public class GetPermissionStatisticsQueryHandler : IRequestHandler<GetPermissionStatisticsQuery, Result<PermissionStatisticsDto>>
{
    private readonly IdentityServiceDbContext _context;

    public GetPermissionStatisticsQueryHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PermissionStatisticsDto>> Handle(GetPermissionStatisticsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var statistics = new PermissionStatisticsDto
            {
                TotalPermissions = await _context.Permissions.CountAsync(cancellationToken),
                TotalRoles = await _context.Roles.CountAsync(cancellationToken),
                TotalUsers = await _context.Users.CountAsync(cancellationToken),
                LastUpdated = DateTime.UtcNow
            };

            // 按分组统计权限数量
            var permissionsByGroup = await _context.Permissions
                .GroupBy(p => p.Group)
                .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
            statistics.PermissionsByGroup = permissionsByGroup;

            // 统计各角色的权限数量
            var rolesPermissionCount = await _context.Roles
                .Include(r => r.Permissions)
                .ToDictionaryAsync(
                    r => r.Name ?? "Unknown", 
                    r => r.Permissions.Count, 
                    cancellationToken);
            statistics.RolesPermissionCount = rolesPermissionCount;

            return Result<PermissionStatisticsDto>.SuccessResult(statistics);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching permission statistics: {ex.Message}");
            return Result<PermissionStatisticsDto>.FailResult("获取权限统计信息失败");
        }
    }
}