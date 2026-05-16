using MediatR;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Roles.Queries;

/// <summary>
/// 创建角色的命令
/// </summary>
public record CreateRoleCommand(
    string Name,
    string Code,
    string? Description = null,
    bool IsEnabled = true
) : IRequest<Result<RoleDto>>;

/// <summary>
/// 更新角色的命令
/// </summary>
public record UpdateRoleCommand(
    Guid Id,
    string? Name = null,
    string? Code = null,
    string? Description = null,
    bool? IsEnabled = null,
    List<Guid>? PermissionIds = null
) : IRequest<Result<bool>>;

/// <summary>
/// 删除角色的命令
/// </summary>
public record DeleteRoleCommand(
    Guid Id
) : IRequest<Result<bool>>;