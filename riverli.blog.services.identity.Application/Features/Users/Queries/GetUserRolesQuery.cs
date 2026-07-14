using MediatR;

namespace riverli.blog.services.identity.Application.Features.Users.Queries;

/// <summary>
/// 查询指定用户绑定的所有角色名称
/// </summary>
public record GetUserRolesQuery(Guid UserId) : IRequest<List<string>>;
