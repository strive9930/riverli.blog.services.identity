using MediatR;
using RiverLi.Blog.Identity.Application.Users.Queries.GetAllUsers;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Users.Queries.GetUserRoles;

/// <summary>
/// 获取用户角色列表查询
/// </summary>
public record GetUserRolesQuery(
    Guid UserId
) : IRequest<Result<List<RoleDto>>>;
