using MediatR;
using Microsoft.AspNetCore.Identity;
using riverli.blog.services.identity.Domain.Entities;

namespace riverli.blog.services.identity.Application.Features.Roles.Commands;

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, bool>
{
    private readonly RoleManager<AppRole> _roleManager;

    public UpdateRoleCommandHandler(RoleManager<AppRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<bool> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(request.Id.ToString());
        if (role == null)
            throw new KeyNotFoundException("角色不存在");

        role.Name = request.Data.Name;
        role.Description = request.Data.Description;

        var result = await _roleManager.UpdateAsync(role);
        return result.Succeeded;
    }
}
