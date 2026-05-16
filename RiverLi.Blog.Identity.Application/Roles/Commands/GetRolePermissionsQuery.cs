using MediatR;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Roles.Commands;

/// <summary>
/// 获取角色权限的查询命令
/// </summary>
public record GetRolePermissionsQuery(
    Guid RoleId
) : IRequest<Result<List<Permission>>>;