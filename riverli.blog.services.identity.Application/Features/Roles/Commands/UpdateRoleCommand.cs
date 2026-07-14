using MediatR;
using riverli.blog.services.identity.Application.DTOs.Roles;

namespace riverli.blog.services.identity.Application.Features.Roles.Commands;

public record UpdateRoleCommand(Guid Id, UpdateRoleDto Data) : IRequest<bool>;
