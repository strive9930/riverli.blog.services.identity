using MediatR;

namespace riverli.blog.services.identity.Application.Features.Users;

public record AssignRolesToUserCommand(Guid UserId, List<string> RoleIds) : IRequest<bool>;
