using MediatR;
using riverli.blog.services.identity.Application.DTOs.Users;

namespace riverli.blog.services.identity.Application.Features.Users.Commands;

public record CreateUserCommand(CreateUserDto Data) : IRequest<bool>;
