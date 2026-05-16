using MediatR;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Users.Commands.CreateUser;

/// <summary>
/// 创建用户命令
/// </summary>
public record CreateUserCommand(
    string Email,
    string Password,
    string? NickName = null,
    string? PhoneNumber = null,
    bool IsEnabled = true
) : IRequest<Result<Guid>>;
