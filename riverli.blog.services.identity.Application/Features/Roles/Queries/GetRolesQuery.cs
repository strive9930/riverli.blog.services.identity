using MediatR;
using riverli.blog.services.identity.Application.DTOs.Roles;

namespace riverli.blog.services.identity.Application.Features.Roles.Queries;

public record GetRolesQuery : IRequest<List<RoleDto>>;
