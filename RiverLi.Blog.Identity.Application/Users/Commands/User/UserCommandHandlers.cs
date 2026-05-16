using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Application.Common.Interfaces;
using RiverLi.Blog.Identity.Application.Users.Commands.User;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Users.Commands.User;

#region 用户命令处理器

/// <summary>
/// 创建用户的命令处理器
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
                CreateTime = DateTime.UtcNow,
                IsEnabled = request.IsEnabled,
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<Guid>.FailResult($"创建用户失败: {errors}");
            }

            return Result<Guid>.SuccessResult(user.Id);
        }
        catch (Exception ex)
        {
            return Result<Guid>.FailResult($"创建用户时发生错误: {ex.Message}");
        }
    }
}

/// <summary>
/// 更新用户的命令处理器
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
                user.UserName = request.Email; // 通常用户名和邮箱保持一致
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
                return Result<bool>.FailResult($"更新用户失败: {errors}");
            }

            return Result<bool>.SuccessResult(true, "用户更新成功");
        }
        catch (Exception ex)
        {
            return Result<bool>.FailResult($"更新用户时发生错误: {ex.Message}");
        }
    }
}
/// <summary>
/// 更新用户密码
/// </summary>
public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result<bool>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ChangePasswordCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<bool>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            return Result<bool>.FailResult("用户不存在");
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result<bool>.FailResult($"密码修改失败: {errors}");
        }

        return Result<bool>.SuccessResult(true, "密码修改成功");
    }
}
/// <summary>
/// 删除用户的命令处理器
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
                return Result<bool>.FailResult($"删除用户失败: {errors}");
            }

            return Result<bool>.SuccessResult(true, "用户删除成功");
        }
        catch (Exception ex)
        {
            return Result<bool>.FailResult($"删除用户时发生错误: {ex.Message}");
        }
    }
}

/// <summary>
/// 为用户分配角色的命令处理器
/// </summary>
public class AssignUserRolesCommandHandler : IRequestHandler<AssignUserRolesCommand, Result<bool>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IdentityServiceDbContext _context;

    public AssignUserRolesCommandHandler(
        UserManager<ApplicationUser> userManager,
        IdentityServiceDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<Result<bool>> Handle(AssignUserRolesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                return Result<bool>.FailResult("用户不存在");
            }

            // 获取要分配的角色名称
            var roles = await _context.Roles
                .Where(r => request.RoleIds.Contains(r.Id))
                .Select(r => r.Name!)
                .ToListAsync(cancellationToken);

            if (roles.Count != request.RoleIds.Count)
            {
                return Result<bool>.FailResult("部分角色不存在");
            }

            // 移除用户当前的所有角色
            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            
            if (!removeResult.Succeeded)
            {
                var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                return Result<bool>.FailResult($"移除用户角色失败: {errors}");
            }

            // 添加新角色
            var addResult = await _userManager.AddToRolesAsync(user, roles);
            
            if (!addResult.Succeeded)
            {
                var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                return Result<bool>.FailResult($"分配用户角色失败: {errors}");
            }

            return Result<bool>.SuccessResult(true, "角色分配成功");
        }
        catch (Exception ex)
        {
            return Result<bool>.FailResult($"分配用户角色时发生错误: {ex.Message}");
        }
    }
}

/// <summary>
/// 记录用户登录历史的命令处理器
/// </summary>
public class RecordUserLoginHistoryCommandHandler : IRequestHandler<RecordUserLoginHistoryCommand, Result<bool>>
{
    private readonly ILoginHistoryService _loginHistoryService;

    public RecordUserLoginHistoryCommandHandler(ILoginHistoryService loginHistoryService)
    {
        _loginHistoryService = loginHistoryService;
    }

    public async Task<Result<bool>> Handle(RecordUserLoginHistoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _loginHistoryService.RecordLoginHistoryAsync(
                request.UserId,
                request.IpAddress,
                request.UserAgent,
                request.Status,
                request.FailureReason,
                request.SessionId);

            return Result<bool>.SuccessResult(true, "登录历史记录成功");
        }
        catch (Exception ex)
        {
            return Result<bool>.FailResult($"记录登录历史失败: {ex.Message}");
        }
    }
}

#endregion