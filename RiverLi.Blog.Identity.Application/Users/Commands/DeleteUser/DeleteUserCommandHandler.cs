using MediatR;
using Microsoft.AspNetCore.Identity;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Users.Commands.DeleteUser;

/// <summary>
/// 删除用户命令处理器
/// </summary>
public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result<bool>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DeleteUserCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<bool>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.Id.ToString());
            if (user == null)
            {
                return Result<bool>.FailResult("用户不存在");
            }

            var result = await _userManager.DeleteAsync(user);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<bool>.FailResult($"删除用户失败：{errors}");
            }

            return Result<bool>.SuccessResult(true, "用户删除成功");
        }
        catch (Exception ex)
        {
            return Result<bool>.FailResult($"删除用户时发生错误：{ex.Message}");
        }
    }
}
