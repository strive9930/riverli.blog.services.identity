using MediatR;

namespace riverli.blog.services.identity.Application.Features.Roles.Commands;

public record UpdateRoleApisCommand(Guid RoleId, List<Guid> ApiIds) : IRequest<bool>;
