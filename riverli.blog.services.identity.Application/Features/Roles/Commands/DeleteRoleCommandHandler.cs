using MediatR;
using Microsoft.AspNetCore.Identity;
using riverli.blog.services.identity.Domain.Entities;

namespace riverli.blog.services.identity.Application.Features.Roles.Commands;

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, bool>
{
    private readonly RoleManager<AppRole> _roleManager;

    public DeleteRoleCommandHandler(RoleManager<AppRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<bool> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(request.Id.ToString());
        if (role == null)
            throw new KeyNotFoundException("角色不存在");

        // 内置超级管理员角色禁止删除
        if (role.Name == "Admin")
            throw new InvalidOperationException("内置超级管理员角色禁止删除");

        var result = await _roleManager.DeleteAsync(role);
        return result.Succeeded;
    }
}
