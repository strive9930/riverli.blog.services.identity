using MediatR;
using Microsoft.AspNetCore.Identity;
using riverli.blog.services.identity.Application.DTOs.Roles;
using riverli.blog.services.identity.Domain.Entities;

namespace riverli.blog.services.identity.Application.Features.Roles.Queries;

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, List<RoleDto>>
{
    private readonly RoleManager<AppRole> _roleManager;

    public GetRolesQueryHandler(RoleManager<AppRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<List<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = _roleManager.Roles.ToList();
        return roles.Select(r => new RoleDto(r.Id, r.Name!, r.Description)).ToList();
    }
}
