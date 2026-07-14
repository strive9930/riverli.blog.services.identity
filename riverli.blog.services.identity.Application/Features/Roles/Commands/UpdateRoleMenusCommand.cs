using MediatR;

namespace riverli.blog.services.identity.Application.Features.Roles.Commands;

public record UpdateRoleMenusCommand(Guid RoleId, List<Guid> MenuIds) : IRequest<bool>;
