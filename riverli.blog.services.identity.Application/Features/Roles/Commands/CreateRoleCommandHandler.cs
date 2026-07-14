using MediatR;
using Microsoft.AspNetCore.Identity;
using riverli.blog.services.identity.Domain.Entities;

namespace riverli.blog.services.identity.Application.Features.Roles.Commands;

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, bool>
{
    private readonly RoleManager<AppRole> _roleManager;

    public CreateRoleCommandHandler(RoleManager<AppRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<bool> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = new AppRole
        {
            Name = request.Data.Name,
            Description = request.Data.Description
        };

        var result = await _roleManager.CreateAsync(role);
        return result.Succeeded;
    }
}
