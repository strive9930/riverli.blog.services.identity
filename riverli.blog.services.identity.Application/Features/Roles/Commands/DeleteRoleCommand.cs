using MediatR;

namespace riverli.blog.services.identity.Application.Features.Roles.Commands;

public record DeleteRoleCommand(Guid Id) : IRequest<bool>;
