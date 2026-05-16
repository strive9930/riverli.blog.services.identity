using MediatR;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Roles.Commands;

/// <summary>
/// 更新用户角色的命令
/// </summary>
public record UpdateUserRolesCommand(
    Guid UserId,
    List<string> RoleNames
) : IRequest<Result<bool>>;