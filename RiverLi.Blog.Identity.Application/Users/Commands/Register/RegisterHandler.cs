using MediatR;
using Microsoft.AspNetCore.Identity;
using RiverLi.DDD.Core.Application.Common.Models;// 包含 Result 泛型类
using RiverLi.Blog.Identity.Domain.Entities;

namespace RiverLi.Blog.Identity.Application.Users.Commands.Register
{
    // 定义 Handler
    public class RegisterHandler : IRequestHandler<RegisterCommand, Result<Guid>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public RegisterHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Result<Guid>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // 1. 检查邮箱是否已被占用
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return Result<Guid>.FailResult("该邮箱已被注册，请直接登录。");
            }

            // 2. 映射领域实体
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = request.Email,
                Email = request.Email,
                NickName = request.NickName,
                CreateTime = DateTime.UtcNow
            };

            // 3. 执行创建（Identity 会自动处理密码哈希和入库）
            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                // 提取 Identity 内部的错误信息（如密码太简单等）
                var errorMsg = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<Guid>.FailResult(errorMsg);
            }

            // 4. 返回成功及新用户 ID
            await _userManager.AddToRoleAsync(user, "User"); 
            return Result<Guid>.SuccessResult(user.Id);
        }
    }
}