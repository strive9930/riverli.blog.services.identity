using MediatR;

namespace riverli.blog.services.identity.Application.Features.Users.Commands;

/// <summary>
/// 删除用户命令
/// 继承 IRequest<bool> 表示操作完成后返回一个布尔值代表是否成功
/// </summary>
public record DeleteUserCommand(Guid UserId) : IRequest<bool>;