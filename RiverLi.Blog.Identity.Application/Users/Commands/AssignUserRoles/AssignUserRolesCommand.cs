using MediatR;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Users.Commands.AssignUserRoles;

/// <summary>
/// 为用户分配角色命令
/// </summary>
public record AssignUserRolesCommand(
    Guid UserId,
    List<Guid> RoleIds
) : IRequest<Result<bool>>;
