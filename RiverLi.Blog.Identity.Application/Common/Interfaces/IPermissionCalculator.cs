using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Common.Interfaces;

/// <summary>
/// 权限计算器接口 - 抽象复杂的权限计算逻辑
/// </summary>
public interface IPermissionCalculator
{
    /// <summary>
    /// 计算用户最终权限集合（考虑角色继承和权限合并）
    /// </summary>
    /// <param name="user">用户实体</param>
    /// <param name="includeRoleInheritance">是否包含角色继承</param>
    /// <returns>用户最终权限列表</returns>
    Task<Result<List<Permission>>> CalculateUserPermissionsAsync(ApplicationUser user, bool includeRoleInheritance = true);

    /// <summary>
    /// 检测权限分配冲突
    /// </summary>
    /// <param name="targetId">目标ID（用户ID或角色ID）</param>
    /// <param name="targetType">目标类型（user或role）</param>
    /// <param name="newPermissionIds">新分配的权限ID列表</param>
    /// <returns>冲突检测结果</returns>
    Task<Result<PermissionConflictResult>> DetectPermissionConflictsAsync(
        Guid targetId, 
        string targetType, 
        List<Guid> newPermissionIds);

    /// <summary>
    /// 验证权限继承关系是否有效
    /// </summary>
    /// <param name="roleId">角色ID</param>
    /// <param name="parentRoleId">父角色ID</param>
    /// <returns>验证结果</returns>
    Task<Result<bool>> ValidateRoleInheritanceAsync(Guid roleId, Guid parentRoleId);

    /// <summary>
    /// 获取权限继承链
    /// </summary>
    /// <param name="roleId">起始角色ID</param>
    /// <returns>权限继承链</returns>
    Task<Result<List<RoleInheritanceChain>>> GetPermissionInheritanceChainAsync(Guid roleId);
}

/// <summary>
/// 权限冲突检测结果
/// </summary>
public class PermissionConflictResult
{
    /// <summary>
    /// 是否存在冲突
    /// </summary>
    public bool HasConflicts { get; set; }
    
    /// <summary>
    /// 冲突的权限列表
    /// </summary>
    public List<PermissionConflictInfo> Conflicts { get; set; } = new();
    
    /// <summary>
    /// 建议的解决方案
    /// </summary>
    public List<string> ResolutionSuggestions { get; set; } = new();
}

/// <summary>
/// 权限冲突信息
/// </summary>
public class PermissionConflictInfo
{
    /// <summary>
    /// 冲突的权限ID
    /// </summary>
    public Guid PermissionId { get; set; }
    
    /// <summary>
    /// 冲突的权限名称
    /// </summary>
    public string PermissionName { get; set; } = string.Empty;
    
    /// <summary>
    /// 冲突原因
    /// </summary>
    public string ConflictReason { get; set; } = string.Empty;
    
    /// <summary>
    /// 冲突来源（哪个角色或用户）
    /// </summary>
    public string ConflictSource { get; set; } = string.Empty;
    
    /// <summary>
    /// 冲突类型
    /// </summary>
    public PermissionConflictType ConflictType { get; set; }
}

/// <summary>
/// 权限冲突类型枚举
/// </summary>
public enum PermissionConflictType
{
    /// <summary>
    /// 权限重复分配
    /// </summary>
    DuplicateAssignment,
    
    /// <summary>
    /// 权限继承冲突
    /// </summary>
    InheritanceConflict,
    
    /// <summary>
    /// 权限互斥冲突
    /// </summary>
    MutualExclusion,
    
    /// <summary>
    /// 权限层级冲突
    /// </summary>
    HierarchyConflict
}

/// <summary>
/// 角色继承链
/// </summary>
public class RoleInheritanceChain
{
    /// <summary>
    /// 角色ID
    /// </summary>
    public Guid RoleId { get; set; }
    
    /// <summary>
    /// 角色名称
    /// </summary>
    public string RoleName { get; set; } = string.Empty;
    
    /// <summary>
    /// 继承层级
    /// </summary>
    public int Level { get; set; }
    
    /// <summary>
    /// 父角色ID
    /// </summary>
    public Guid? ParentRoleId { get; set; }
    
    /// <summary>
    /// 继承类型
    /// </summary>
    public InheritanceType InheritanceType { get; set; }
    
    /// <summary>
    /// 该角色拥有的权限数量
    /// </summary>
    public int PermissionCount { get; set; }
}

/// <summary>
/// 继承类型枚举
/// </summary>
public enum InheritanceType
{
    /// <summary>
    /// 直接分配
    /// </summary>
    Direct,
    
    /// <summary>
    /// 继承获得
    /// </summary>
    Inherited,
    
    /// <summary>
    /// 混合（既有直接又有继承）
    /// </summary>
    Mixed
}