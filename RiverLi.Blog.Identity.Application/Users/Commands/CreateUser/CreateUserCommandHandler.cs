using MediatR;
using Microsoft.AspNetCore.Identity;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Users.Commands.CreateUser;

/// <summary>
/// 创建用户命令处理器
/// </summary>
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public CreateUserCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 检查邮箱是否已存在
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return Result<Guid>.FailResult("该邮箱已被注册");
            }

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = request.Email,
                Email = request.Email,
                NickName = request.NickName,
                PhoneNumber = request.PhoneNumber,
                CreateTime = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<Guid>.FailResult($"创建用户失败：{errors}");
            }

            return Result<Guid>.SuccessResult(user.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.FailResult($"创建用户时发生错误：{ex.Message}");
        }
    }
}
