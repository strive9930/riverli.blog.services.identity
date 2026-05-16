using MediatR;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Users.Commands.UpdateUser;

/// <summary>
/// 更新用户命令
/// </summary>
public record UpdateUserCommand(
    Guid Id,
    string? Email = null,
    string? NickName = null,
    string? PhoneNumber = null,
    bool? IsEnabled = null
) : IRequest<Result<bool>>;
