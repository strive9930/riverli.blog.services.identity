using MediatR;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Roles.Commands;

/// <summary>
/// 获取所有角色的查询命令
/// </summary>
public record GetAllRolesQuery(
    string? Keyword = null,
    bool? IsEnabled = null
) : IRequest<Result<List<RoleDto>>>;

/// <summary>
/// 获取角色详情的查询命令
/// </summary>
public record GetRoleByIdQuery(
    Guid RoleId
) : IRequest<Result<RoleDetailDto>>;

/// <summary>
/// 角色 DTO
/// 用于传输角色的基础信息
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
    /// 角色编码（用于代码中引用）
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// 角色描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }
    
    /// <summary>
    /// 用户数量（拥有该角色的用户数）
    /// </summary>
    public int UserCount { get; set; }
    
    /// <summary>
    /// 权限数量（该角色分配的权限数）
    /// </summary>
    public int PermissionCount { get; set; }
    
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
/// 角色详情 DTO
/// 继承自 RoleDto，包含权限和用户的详细信息
/// </summary>
public class RoleDetailDto : RoleDto
{
    /// <summary>
    /// 该角色拥有的权限列表
    /// </summary>
    public List<PermissionDto> Permissions { get; set; } = new();
    
    /// <summary>
    /// 拥有该角色的用户列表
    /// </summary>
    public List<UserDto> Users { get; set; } = new();
}

/// <summary>
/// 权限 DTO（简化版）
/// 用于角色详情中展示权限信息
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

/// <summary>
/// 用户 DTO（简化版）
/// 用于角色详情中展示用户信息
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
    
    /// <summary>
    /// 用户昵称
    /// </summary>
    public string? NickName { get; set; }
}