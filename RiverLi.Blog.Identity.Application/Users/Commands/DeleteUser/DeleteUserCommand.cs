using MediatR;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Users.Commands.DeleteUser;

/// <summary>
/// 删除用户命令
/// </summary>
public record DeleteUserCommand(
    Guid Id
) : IRequest<Result<bool>>;
