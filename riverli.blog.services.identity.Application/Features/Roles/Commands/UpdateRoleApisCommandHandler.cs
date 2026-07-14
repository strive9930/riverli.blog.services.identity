using MediatR;
using riverli.blog.services.identity.Application.Interfaces;

namespace riverli.blog.services.identity.Application.Features.Roles.Commands;

public class UpdateRoleApisCommandHandler : IRequestHandler<UpdateRoleApisCommand, bool>
{
    private readonly IRoleRepository _roleRepository;

    public UpdateRoleApisCommandHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<bool> Handle(UpdateRoleApisCommand request, CancellationToken cancellationToken)
    {
        await _roleRepository.UpdateRoleApisAsync(request.RoleId, request.ApiIds, cancellationToken);
        return true;
    }
}
