using MediatR;

namespace riverli.blog.services.identity.Application.Features.Roles.Queries;

public record GetRoleMenusQuery(Guid RoleId) : IRequest<List<Guid>>;
