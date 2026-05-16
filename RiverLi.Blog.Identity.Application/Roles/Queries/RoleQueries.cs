using MediatR;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Roles.Queries;

/// <summary>
/// 获取所有角色的查询命令
/// </summary>
public record GetAllRolesQuery(
    string? Keyword = null,
    bool? IsEnabled = null
) : IRequest<Result<List<RoleDto>>>;

/// <summary>
/// 分页获取角色列表的查询命令
/// </summary>
public record GetPagedRolesQuery(
    int PageIndex = 1,
    int PageSize = 10,
    string? Keyword = null,
    bool? IsEnabled = null,
    string? SortBy = "Name",
    bool SortDesc = false
) : IRequest<PagedResult<RoleDto>>;

/// <summary>
/// 获取角色详情的查询命令
/// </summary>
public record GetRoleByIdQuery(
    Guid RoleId
) : IRequest<Result<RoleDetailDto>>;

/// <summary>
/// 角色DTO
/// </summary>
public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }
    public int UserCount { get; set; }
    public int PermissionCount { get; set; }
    public DateTime? CreateTime { get; set; }
    public DateTime? UpdateTime { get; set; }
}

/// <summary>
/// 角色详情DTO
/// </summary>
public class RoleDetailDto : RoleDto
{
    public List<PermissionDto> Permissions { get; set; } = new();
    public List<UserDto> Users { get; set; } = new();
}

/// <summary>
/// 权限DTO
/// </summary>
public class PermissionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Group { get; set; } = string.Empty;
}

/// <summary>
/// 用户DTO
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? NickName { get; set; }
}