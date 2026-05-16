using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Application.Common.Interfaces;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Common.Services;

/// <summary>
/// 权限计算器实现类
/// </summary>
public class PermissionCalculator : IPermissionCalculator
{
    private readonly IdentityServiceDbContext _context;

    public PermissionCalculator(IdentityServiceDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 计算用户最终权限集合
    /// </summary>
    public async Task<Result<List<Permission>>> CalculateUserPermissionsAsync(ApplicationUser user, bool includeRoleInheritance = true)
    {
        try
        {
            var allPermissions = new HashSet<Permission>(new PermissionComparer());

            // 1. 获取用户直接分配的权限
            var userDirectPermissions = await _context.Users
                .Where(u => u.Id == user.Id)
                .SelectMany(u => u.Permissions)
                .ToListAsync();

            foreach (var permission in userDirectPermissions)
            {
                allPermissions.Add(permission);
            }

            // 2. 获取用户通过角色获得的权限
            var userRoles = await _context.Users
                .Where(u => u.Id == user.Id)
                .SelectMany(u => u.Roles)
                .ToListAsync();

            foreach (var role in userRoles)
            {
                var rolePermissions = await GetRolePermissionsWithInheritance(role.Id, includeRoleInheritance);
                foreach (var permission in rolePermissions)
                {
                    allPermissions.Add(permission);
                }
            }

            return Result<List<Permission>>.SuccessResult(allPermissions.ToList());
        }
        catch (Exception ex)
        {
            return Result<List<Permission>>.FailResult($"计算用户权限失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 检测权限分配冲突
    /// </summary>
    public async Task<Result<PermissionConflictResult>> DetectPermissionConflictsAsync(
        Guid targetId, 
        string targetType, 
        List<Guid> newPermissionIds)
    {
        try
        {
            var result = new PermissionConflictResult { HasConflicts = false };
            
            if (targetType.ToLower() == "user")
            {
                await DetectUserPermissionConflicts(targetId, newPermissionIds, result);
            }
            else if (targetType.ToLower() == "role")
            {
                await DetectRolePermissionConflicts(targetId, newPermissionIds, result);
            }

            // 生成解决方案建议
            GenerateResolutionSuggestions(result);

            return Result<PermissionConflictResult>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            return Result<PermissionConflictResult>.FailResult($"检测权限冲突失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 验证角色继承关系
    /// </summary>
    public async Task<Result<bool>> ValidateRoleInheritanceAsync(Guid roleId, Guid parentRoleId)
    {
        try
        {
            // 检查是否会造成循环继承
            var inheritanceChain = await GetInheritanceChain(parentRoleId);
            if (inheritanceChain.Any(r => r.Id == roleId))
            {
                return Result<bool>.FailResult("不能建立循环继承关系");
            }

            // 检查角色是否存在
            var roleExists = await _context.Roles.AnyAsync(r => r.Id == roleId);
            var parentRoleExists = await _context.Roles.AnyAsync(r => r.Id == parentRoleId);
            
            if (!roleExists || !parentRoleExists)
            {
                return Result<bool>.FailResult("角色不存在");
            }

            return Result<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.FailResult($"验证角色继承失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取权限继承链
    /// </summary>
    public async Task<Result<List<RoleInheritanceChain>>> GetPermissionInheritanceChainAsync(Guid roleId)
    {
        try
        {
            var chain = new List<RoleInheritanceChain>();
            await BuildInheritanceChain(roleId, chain, 0, null);
            return Result<List<RoleInheritanceChain>>.SuccessResult(chain);
        }
        catch (Exception ex)
        {
            return Result<List<RoleInheritanceChain>>.FailResult($"获取继承链失败: {ex.Message}");
        }
    }

    #region 私有辅助方法

    /// <summary>
    /// 获取角色及其继承角色的权限
    /// </summary>
    private async Task<List<Permission>> GetRolePermissionsWithInheritance(Guid roleId, bool includeInheritance)
    {
        var permissions = new List<Permission>();

        // 获取角色直接权限
        var role = await _context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);

        if (role != null)
        {
            permissions.AddRange(role.Permissions);

            // 如果包含继承，获取父角色权限
            if (includeInheritance && role.ParentRoleId.HasValue)
            {
                var parentPermissions = await GetRolePermissionsWithInheritance(role.ParentRoleId.Value, true);
                permissions.AddRange(parentPermissions);
            }
        }

        return permissions.Distinct(new PermissionComparer()).ToList();
    }

    /// <summary>
    /// 检测用户权限冲突
    /// </summary>
    private async Task DetectUserPermissionConflicts(Guid userId, List<Guid> newPermissionIds, PermissionConflictResult result)
    {
        // 检查重复分配
        var existingPermissions = await _context.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Permissions)
            .Select(p => p.Id)
            .ToListAsync();

        var duplicates = newPermissionIds.Intersect(existingPermissions).ToList();
        foreach (var duplicateId in duplicates)
        {
            var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.Id == duplicateId);
            if (permission != null)
            {
                result.Conflicts.Add(new PermissionConflictInfo
                {
                    PermissionId = permission.Id,
                    PermissionName = permission.Name,
                    ConflictReason = "权限已存在，无需重复分配",
                    ConflictSource = "用户直接权限",
                    ConflictType = PermissionConflictType.DuplicateAssignment
                });
            }
        }

        // 检查通过角色继承的权限冲突
        var userRoles = await _context.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Roles)
            .Select(r => r.Id)
            .ToListAsync();

        foreach (var roleId in userRoles)
        {
            var rolePermissions = await GetRolePermissionIds(roleId);
            var inheritedConflicts = newPermissionIds.Intersect(rolePermissions).ToList();
            
            foreach (var conflictId in inheritedConflicts)
            {
                var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.Id == conflictId);
                if (permission != null)
                {
                    result.Conflicts.Add(new PermissionConflictInfo
                    {
                        PermissionId = permission.Id,
                        PermissionName = permission.Name,
                        ConflictReason = "权限已通过角色继承获得",
                        ConflictSource = $"角色ID: {roleId}",
                        ConflictType = PermissionConflictType.InheritanceConflict
                    });
                }
            }
        }

        result.HasConflicts = result.Conflicts.Any();
    }

    /// <summary>
    /// 检测角色权限冲突
    /// </summary>
    private async Task DetectRolePermissionConflicts(Guid roleId, List<Guid> newPermissionIds, PermissionConflictResult result)
    {
        // 检查重复分配
        var existingPermissions = await _context.Roles
            .Where(r => r.Id == roleId)
            .SelectMany(r => r.Permissions)
            .Select(p => p.Id)
            .ToListAsync();

        var duplicates = newPermissionIds.Intersect(existingPermissions).ToList();
        foreach (var duplicateId in duplicates)
        {
            var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.Id == duplicateId);
            if (permission != null)
            {
                result.Conflicts.Add(new PermissionConflictInfo
                {
                    PermissionId = permission.Id,
                    PermissionName = permission.Name,
                    ConflictReason = "权限已存在，无需重复分配",
                    ConflictSource = "角色直接权限",
                    ConflictType = PermissionConflictType.DuplicateAssignment
                });
            }
        }

        // 检查父角色权限冲突
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
        if (role?.ParentRoleId != null)
        {
            var parentPermissions = await GetRolePermissionIds(role.ParentRoleId.Value);
            var inheritedConflicts = newPermissionIds.Intersect(parentPermissions).ToList();
            
            foreach (var conflictId in inheritedConflicts)
            {
                var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.Id == conflictId);
                if (permission != null)
                {
                    result.Conflicts.Add(new PermissionConflictInfo
                    {
                        PermissionId = permission.Id,
                        PermissionName = permission.Name,
                        ConflictReason = "权限已通过父角色继承获得",
                        ConflictSource = $"父角色ID: {role.ParentRoleId}",
                        ConflictType = PermissionConflictType.InheritanceConflict
                    });
                }
            }
        }

        result.HasConflicts = result.Conflicts.Any();
    }

    /// <summary>
    /// 获取角色权限ID列表（包括继承）
    /// </summary>
    private async Task<List<Guid>> GetRolePermissionIds(Guid roleId)
    {
        var permissionIds = new List<Guid>();

        var role = await _context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);

        if (role != null)
        {
            permissionIds.AddRange(role.Permissions.Select(p => p.Id));

            // 递归获取父角色权限
            if (role.ParentRoleId.HasValue)
            {
                var parentPermissionIds = await GetRolePermissionIds(role.ParentRoleId.Value);
                permissionIds.AddRange(parentPermissionIds);
            }
        }

        return permissionIds.Distinct().ToList();
    }

    /// <summary>
    /// 获取继承链
    /// </summary>
    private async Task<List<ApplicationRole>> GetInheritanceChain(Guid roleId)
    {
        var chain = new List<ApplicationRole>();
        var currentRoleId = roleId;

        while (currentRoleId != Guid.Empty)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == currentRoleId);
            if (role == null) break;

            chain.Add(role);
            
            if (role.ParentRoleId.HasValue)
            {
                currentRoleId = role.ParentRoleId.Value;
            }
            else
            {
                break;
            }
        }

        return chain;
    }

    /// <summary>
    /// 构建继承链
    /// </summary>
    private async Task BuildInheritanceChain(Guid roleId, List<RoleInheritanceChain> chain, int level, Guid? parentId)
    {
        var role = await _context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);

        if (role != null)
        {
            chain.Add(new RoleInheritanceChain
            {
                RoleId = role.Id,
                RoleName = role.Name ?? "未知角色",
                Level = level,
                ParentRoleId = parentId,
                InheritanceType = parentId.HasValue ? InheritanceType.Inherited : InheritanceType.Direct,
                PermissionCount = role.Permissions.Count
            });

            // 递归处理父角色
            if (role.ParentRoleId.HasValue)
            {
                await BuildInheritanceChain(role.ParentRoleId.Value, chain, level + 1, role.Id);
            }
        }
    }

    /// <summary>
    /// 生成解决方案建议
    /// </summary>
    private void GenerateResolutionSuggestions(PermissionConflictResult result)
    {
        if (!result.HasConflicts) return;

        var duplicateConflicts = result.Conflicts.Count(c => c.ConflictType == PermissionConflictType.DuplicateAssignment);
        var inheritanceConflicts = result.Conflicts.Count(c => c.ConflictType == PermissionConflictType.InheritanceConflict);

        if (duplicateConflicts > 0)
        {
            result.ResolutionSuggestions.Add("移除重复分配的权限，避免权限冗余");
        }

        if (inheritanceConflicts > 0)
        {
            result.ResolutionSuggestions.Add("考虑调整角色继承关系，优化权限层次结构");
            result.ResolutionSuggestions.Add("可以通过角色继承获得的权限无需重复分配");
        }

        result.ResolutionSuggestions.Add("建议定期审查权限分配，确保权限体系清晰合理");
    }

    #endregion
}

/// <summary>
/// 权限比较器
/// </summary>
public class PermissionComparer : IEqualityComparer<Permission?>
{
    public bool Equals(Permission? x, Permission? y)
    {
        if (x == null && y == null) return true;
        if (x == null || y == null) return false;
        return x.Id == y.Id;
    }

    public int GetHashCode(Permission? obj)
    {
        return obj?.Id.GetHashCode() ?? 0;
    }
}