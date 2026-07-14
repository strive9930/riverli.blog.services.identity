using MediatR;
using Microsoft.AspNetCore.Identity;
using riverli.blog.services.identity.Domain.Entities;

namespace riverli.blog.services.identity.Application.Features.Users.Commands;

public class AssignRolesToUserCommandHandler : IRequestHandler<AssignRolesToUserCommand, bool>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    public AssignRolesToUserCommandHandler(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<bool> Handle(AssignRolesToUserCommand request, CancellationToken cancellationToken)
    {
        // 通过 UserManager 加载用户，确保实体被 UserManager 的 DbContext 追踪
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null) throw new KeyNotFoundException("用户不存在");

        // 兼容历史数据：SecurityStamp 为空时自动补全
        if (string.IsNullOrEmpty(user.SecurityStamp))
        {
            await _userManager.UpdateSecurityStampAsync(user);
        }

        // 根据角色 ID 列表查找对应的角色名称
        var roleNames = new List<string>();
        foreach (var roleId in request.RoleIds)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role != null) roleNames.Add(role.Name!);
        }

        // 核心：先移除旧角色，再添加新角色
        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Any())
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
                throw new InvalidOperationException($"移除旧角色失败: {string.Join(", ", removeResult.Errors.Select(e => e.Description))}");
        }

        if (roleNames.Any())
        {
            var addResult = await _userManager.AddToRolesAsync(user, roleNames);
            if (!addResult.Succeeded)
                throw new InvalidOperationException($"添加新角色失败: {string.Join(", ", addResult.Errors.Select(e => e.Description))}");
        }

        return true;
    }
}