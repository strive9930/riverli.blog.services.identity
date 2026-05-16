using MediatR;
using Microsoft.AspNetCore.Identity;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Users.Commands.UpdateUser;

/// <summary>
/// 更新用户命令处理器
/// </summary>
public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<bool>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UpdateUserCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<bool>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.Id.ToString());
            if (user == null)
            {
                return Result<bool>.FailResult("用户不存在");
            }

            // 更新用户信息
            if (!string.IsNullOrEmpty(request.Email))
            {
                user.Email = request.Email;
                user.UserName = request.Email;
            }

            if (request.NickName != null)
            {
                user.NickName = request.NickName;
            }

            if (request.PhoneNumber != null)
            {
                user.PhoneNumber = request.PhoneNumber;
            }
            
            if (request.IsEnabled != null)
            {
                user.IsEnabled = request.IsEnabled.Value;
            }

            var result = await _userManager.UpdateAsync(user);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<bool>.FailResult($"更新用户失败：{errors}");
            }

            return Result<bool>.SuccessResult(true, "用户更新成功");
        }
        catch (Exception ex)
        {
            return Result<bool>.FailResult($"更新用户时发生错误：{ex.Message}");
        }
    }
}
