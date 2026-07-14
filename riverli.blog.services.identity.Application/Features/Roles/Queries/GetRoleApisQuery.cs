using MediatR;

namespace riverli.blog.services.identity.Application.Features.Roles.Queries;

public record GetRoleApisQuery(Guid RoleId) : IRequest<List<Guid>>;
