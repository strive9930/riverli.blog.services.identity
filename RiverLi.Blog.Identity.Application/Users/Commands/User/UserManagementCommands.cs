using MediatR;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Users.Commands.User;

#region 用户 CRUD 命令

/// <summary>
/// 获取所有用户的查询命令
/// </summary>
public record GetAllUsersQuery(
    string? Keyword = null,
    bool? IsEnabled = null,
    int PageIndex = 1,
    int PageSize = 10
) : IRequest<PagedResult<UserDto>>;

/// <summary>
/// 创建用户的命令
/// </summary>
public record CreateUserCommand(
    string Email,
    string Password,
    string? NickName = null,
    string? PhoneNumber = null,
    bool IsEnabled = true
) : IRequest<Result<Guid>>;

/// <summary>
/// 更新用户的命令
/// </summary>
public record UpdateUserCommand(
    Guid Id,
    string? Email = null,
    string? NickName = null,
    string? PhoneNumber = null,
    bool? IsEnabled = null
) : IRequest<Result<bool>>;

/// <summary>
/// 删除用户的命令
/// </summary>
public record DeleteUserCommand(
    Guid Id
) : IRequest<Result<bool>>;

/// <summary>
/// 获取用户角色的查询命令
/// </summary>
public record GetUserRolesQuery(
    Guid UserId
) : IRequest<Result<List<RoleDto>>>;

/// <summary>
/// 为用户分配角色的命令
/// </summary>
public record AssignUserRolesCommand(
    Guid UserId,
    List<Guid> RoleIds
) : IRequest<Result<bool>>;

/// <summary>
/// 记录用户登录历史的命令
/// </summary>
public record RecordUserLoginHistoryCommand(
    Guid UserId,
    string IpAddress,
    string UserAgent,
    string Status = "Success",
    string? SessionId = null,
    string? FailureReason = null
) : IRequest<Result<bool>>;

#endregion

#region DTO 定义

/// <summary>
/// 用户 DTO
/// 用于展示用户的完整信息（管理端）
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
    /// 邮箱地址
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// 昵称
    /// </summary>
    public string? NickName { get; set; }
    
    /// <summary>
    /// 电话号码
    /// </summary>
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreateTime { get; set; }
    
    /// <summary>
    /// 最后登录时间
    /// </summary>
    public DateTime? LastLoginTime { get; set; }
    
    /// <summary>
    /// 用户角色列表
    /// </summary>
    public List<RoleDto> Roles { get; set; } = new();
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }
}

/// <summary>
/// 角色 DTO（简化版）
/// 用于用户信息中展示角色
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
    /// 角色编码
    /// </summary>
    public string? Code { get; set; }
    
    /// <summary>
    /// 角色描述
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// 权限 DTO（简化版）
/// 用于用户信息中展示权限
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
}

// 注：使用 RiverLi.DDD.Core.Application.Common.Models.PagedResult<T>

#endregion