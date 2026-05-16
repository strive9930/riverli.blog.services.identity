using MediatR;
using RiverLi.Blog.Identity.Application.Permissions.Commands;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Permissions.Queries;

/// <summary>
/// 获取分组权限的查询命令
/// </summary>
public record GetGroupedPermissionsQuery(
    string? SearchKeyword = null
) : IRequest<Result<List<GroupedPermissionsDto>>>;

/// <summary>
/// 获取角色权限状态的查询命令
/// </summary>
public record GetRolePermissionStatusQuery(
    Guid RoleId
) : IRequest<Result<RolePermissionStatusDto>>;

/// <summary>
/// 批量分配权限的命令
/// </summary>
public record BatchAssignPermissionsCommand(
    Guid RoleId,
    List<Guid> AddPermissionIds,
    List<Guid> RemovePermissionIds
) : IRequest<Result<bool>>;

/// <summary>
/// 搜索权限的查询命令
/// </summary>
public record SearchPermissionsQuery(
    string Keyword,
    int PageSize = 20
) : IRequest<Result<List<PermissionSearchResultDto>>>;


/// <summary>
/// 分组权限DTO
/// </summary>
public class GroupedPermissionsDto
{
    public string GroupName { get; set; } = string.Empty;
    public List<PermissionDto> Permissions { get; set; } = new();
}

/// <summary>
/// 角色权限状态DTO
/// 用于表示角色的权限分配状态信息
/// </summary>
public class RolePermissionStatusDto
{
    /// <summary>
    /// 角色唯一标识符
    /// </summary>
    public Guid RoleId { get; set; }
    
    /// <summary>
    /// 角色名称
    /// </summary>
    public string RoleName { get; set; } = string.Empty;
    
    /// <summary>
    /// 已分配给该角色的权限ID列表
    /// </summary>
    public List<Guid> AssignedPermissionIds { get; set; } = new();
    
    /// <summary>
    /// 分配给该角色的用户数量
    /// </summary>
    public int UserCount { get; set; }
    
    /// <summary>
    /// 系统中权限的总数量
    /// </summary>
    public int PermissionCount { get; set; }
    
    /// <summary>
    /// 当前角色已分配的权限数量
    /// </summary>
    public int AssignedPermissions { get; set; }
}

/// <summary>
/// 权限搜索结果DTO
/// </summary>
public class PermissionSearchResultDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public double RelevanceScore { get; set; } // 相关性评分
}

