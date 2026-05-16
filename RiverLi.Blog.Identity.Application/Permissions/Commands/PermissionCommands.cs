using MediatR;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Permissions.Commands;

public record GetAllPermissionsQuery(
    string? Group = null,
    bool? Enabled = null,
    string? Keyword = null
) : IRequest<Result<List<PermissionDto>>>;

public record GetPagedPermissionsQuery(
    int PageIndex = 1,
    int PageSize = 10,
    string? Group = null,
    bool? Enabled = null,
    string? Keyword = null,
    string? SortBy = "Name",
    bool SortDesc = false
) : IRequest<PagedResult<PermissionDto>>;

public record GetPermissionByIdQuery(
    Guid Id
) : IRequest<Result<PermissionDetailDto>>;

public record GetPermissionByCodeQuery(
    string Code
) : IRequest<Result<PermissionDto>>;

public record GetUserPermissionsQuery(
    Guid UserId
) : IRequest<Result<List<PermissionDto>>>;

public record GetRolePermissionsQuery(
    Guid RoleId
) : IRequest<Result<List<PermissionDto>>>;

public record CheckUserPermissionQuery(
    Guid UserId,
    string PermissionCode
) : IRequest<Result<bool>>;

public record GetPermissionTreeQuery(
    bool? Enabled = null
) : IRequest<Result<List<PermissionGroupTreeNodeDto>>>;

public record CreatePermissionCommand(
    string Name,
    string Code,
    string? Description = null,
    string Group = "",
    string? ClaimType = null,
    string? ClaimValue = null
) : IRequest<Result<PermissionDto>>;

public record UpdatePermissionCommand(
    Guid Id,
    string Name,
    string Code,
    string? Description = null,
    string Group = "",
    string? ClaimType = null,
    string? ClaimValue = null,
    bool IsEnabled = true
) : IRequest<Result<PermissionDto>>;

public record DeletePermissionCommand(
    Guid Id
) : IRequest<Result<string>>;

public record BatchDeletePermissionsCommand(
    List<Guid> PermissionIds
) : IRequest<Result<string>>;

public record BatchUpdatePermissionStatusCommand(
    List<Guid> PermissionIds,
    bool IsEnabled
) : IRequest<Result<string>>;

public record EnablePermissionCommand(
    Guid Id
) : IRequest<Result<string>>;

public record DisablePermissionCommand(
    Guid Id
) : IRequest<Result<string>>;

public record GetPermissionGroupsQuery : IRequest<Result<List<PermissionGroupDto>>>;

public record CreatePermissionGroupCommand(
    string Name,
    string Code,
    string? Description = null
) : IRequest<Result<PermissionGroupDto>>;

public record UpdatePermissionGroupCommand(
    string Code,
    string Name,
    string? Description = null
) : IRequest<Result<PermissionGroupDto>>;

public record DeletePermissionGroupCommand(
    string Code
) : IRequest<Result<string>>;

public record GetPermissionsByGroupQuery(
    string GroupCode
) : IRequest<Result<List<PermissionDto>>>;

public record ExportPermissionsQuery(
    string? Group = null
) : IRequest<Result<string>>;

public record ImportPermissionsCommand(
    List<ImportPermissionDto> Permissions
) : IRequest<Result<string>>;

public record BatchAssignPermissionsToRoleCommand(
    Guid RoleId,
    List<Guid> PermissionIds
) : IRequest<Result<string>>;

public record BatchRemoveRolePermissionsCommand(
    Guid RoleId,
    List<Guid> PermissionIds
) : IRequest<Result<string>>;

public record GetPermissionStatisticsQuery : IRequest<Result<PermissionStatisticsDto>>;

public record GetPermissionUsageReportQuery(
    int Days = 30
) : IRequest<Result<PermissionUsageReportDto>>;

public record GetUnassignedPermissionsQuery : IRequest<Result<List<PermissionDto>>>;

/// <summary>
/// 权限 DTO
/// 用于传输权限基础信息
/// </summary>
public class PermissionDto
{
    /// <summary>
    /// 权限唯一标识符
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// 权限名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 权限编码（用于代码中验证）
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// 权限描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 权限所属分组
    /// </summary>
    public string Group { get; set; } = string.Empty;
    
    /// <summary>
    /// 声明类型（用于 JWT 声明）
    /// </summary>
    public string? ClaimType { get; set; }
    
    /// <summary>
    /// 声明值（用于 JWT 声明）
    /// </summary>
    public string? ClaimValue { get; set; }
    
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreateTime { get; set; }
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }
}

/// <summary>
/// 权限详情 DTO
/// 继承自 PermissionDto，包含分配的角色和用户信息
/// </summary>
public class PermissionDetailDto : PermissionDto
{
    /// <summary>
    /// 已分配该权限的角色列表
    /// </summary>
    public List<RoleDto> AssignedRoles { get; set; } = new();
    
    /// <summary>
    /// 已分配该权限的用户列表
    /// </summary>
    public List<UserDto> AssignedUsers { get; set; } = new();
}

/// <summary>
/// 权限分组 DTO
/// 用于展示权限分组的基础信息
/// </summary>
public class PermissionGroupDto
{
    /// <summary>
    /// 分组名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 分组编码
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// 分组描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 该分组下的权限数量
    /// </summary>
    public int PermissionCount { get; set; }
    
    /// <summary>
    /// 分组创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }
}

/// <summary>
/// 角色 DTO
/// 用于传输角色基础信息
/// </summary>
public class RoleDto
{
    /// <summary>
    /// 角色唯一标识符
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// 角色名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 角色描述
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// 用户 DTO
/// 用于传输用户基础信息
/// </summary>
public class UserDto
{
    /// <summary>
    /// 用户唯一标识符
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// 用户邮箱
    /// </summary>
    public string? Email { get; set; }
}

/// <summary>
/// 导入权限 DTO
/// 用于批量导入权限时的数据格式
/// </summary>
public class ImportPermissionDto
{
    /// <summary>
    /// 权限名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 权限编码
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// 权限描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 权限所属分组
    /// </summary>
    public string Group { get; set; } = string.Empty;
    
    /// <summary>
    /// 声明类型
    /// </summary>
    public string? ClaimType { get; set; }
    
    /// <summary>
    /// 声明值
    /// </summary>
    public string? ClaimValue { get; set; }
}

/// <summary>
/// 权限分组树节点 DTO
/// 用于展示权限分组的树形结构
/// </summary>
public class PermissionGroupTreeNodeDto
{
    /// <summary>
    /// 分组名称
    /// </summary>
    public string GroupName { get; set; } = string.Empty;
    
    /// <summary>
    /// 分组编码
    /// </summary>
    public string GroupCode { get; set; } = string.Empty;
    
    /// <summary>
    /// 分组描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 该分组下的权限列表
    /// </summary>
    public List<PermissionDto> Permissions { get; set; } = new();
    
    /// <summary>
    /// 权限总数
    /// </summary>
    public int PermissionCount { get; set; }
}

/// <summary>
/// 权限统计 DTO
/// 用于展示权限系统的统计信息
/// </summary>
public class PermissionStatisticsDto
{
    /// <summary>
    /// 权限总数量
    /// </summary>
    public int TotalPermissions { get; set; }
    
    /// <summary>
    /// 已启用的权限数量
    /// </summary>
    public int EnabledPermissions { get; set; }
    
    /// <summary>
    /// 已禁用的权限数量
    /// </summary>
    public int DisabledPermissions { get; set; }
    
    /// <summary>
    /// 分组总数
    /// </summary>
    public int GroupCount { get; set; }
    
    /// <summary>
    /// 各分组的权限数量统计
    /// </summary>
    public Dictionary<string, int> PermissionsByGroup { get; set; } = new();
    
    /// <summary>
    /// 已分配的权限数量
    /// </summary>
    public int AssignedPermissions { get; set; }
    
    /// <summary>
    /// 未分配的权限数量
    /// </summary>
    public int UnassignedPermissions { get; set; }
    
    /// <summary>
    /// 角色总数
    /// </summary>
    public int TotalRoles { get; set; }
    
    /// <summary>
    /// 用户总数
    /// </summary>
    public int TotalUsers { get; set; }
    
    /// <summary>
    /// 各角色的权限数量统计
    /// </summary>
    public Dictionary<string, int> RolesPermissionCount { get; set; } = new();
    
    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// 权限使用报告 DTO
/// 用于展示权限使用情况
/// </summary>
public class PermissionUsageReportDto
{
    /// <summary>
    /// 报告日期
    /// </summary>
    public DateTime ReportDate { get; set; }
    
    /// <summary>
    /// 活跃用户数
    /// </summary>
    public int ActiveUsers { get; set; }
    
    /// <summary>
    /// 各权限的使用次数统计
    /// </summary>
    public Dictionary<string, int> PermissionUsageCount { get; set; } = new();
    
    /// <summary>
    /// 最常使用的权限列表
    /// </summary>
    public List<PermissionUsageDetailDto> TopUsedPermissions { get; set; } = new();
    
    /// <summary>
    /// 未使用的权限列表
    /// </summary>
    public List<PermissionUsageDetailDto> UnusedPermissions { get; set; } = new();
}

/// <summary>
/// 权限使用详情 DTO
/// 用于展示单个权限的使用详情
/// </summary>
public class PermissionUsageDetailDto
{
    /// <summary>
    /// 权限编码
    /// </summary>
    public string PermissionCode { get; set; } = string.Empty;
    
    /// <summary>
    /// 权限名称
    /// </summary>
    public string PermissionName { get; set; } = string.Empty;
    
    /// <summary>
    /// 使用次数
    /// </summary>
    public int UsageCount { get; set; }
    
    /// <summary>
    /// 最后使用时间
    /// </summary>
    public DateTime LastUsed { get; set; }
}
