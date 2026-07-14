using MediatR;
using riverli.blog.services.identity.Application.DTOs.Roles;

namespace riverli.blog.services.identity.Application.Features.Roles.Commands;

public record CreateRoleCommand(CreateRoleDto Data) : IRequest<bool>;
