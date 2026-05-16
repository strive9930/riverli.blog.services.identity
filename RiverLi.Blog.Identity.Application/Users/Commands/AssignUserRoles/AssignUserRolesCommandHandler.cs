using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Users.Commands.AssignUserRoles;

/// <summary>
/// 为用户分配角色命令处理器
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
                return Result<bool>.FailResult($"移除用户角色失败：{errors}");
            }

            // 添加新角色
            var addResult = await _userManager.AddToRolesAsync(user, roles);
            
            if (!addResult.Succeeded)
            {
                var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                return Result<bool>.FailResult($"分配用户角色失败：{errors}");
            }

            return Result<bool>.SuccessResult(true, "角色分配成功");
        }
        catch (Exception ex)
        {
            return Result<bool>.FailResult($"分配用户角色时发生错误：{ex.Message}");
        }
    }
}
