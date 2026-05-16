using MediatR;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Roles.Commands;

/// <summary>
/// 为角色分配权限的命令
/// </summary>
public record AssignPermissionsToRoleCommand(
    Guid RoleId,
    List<Guid> PermissionIds
) : IRequest<Result<bool>>;