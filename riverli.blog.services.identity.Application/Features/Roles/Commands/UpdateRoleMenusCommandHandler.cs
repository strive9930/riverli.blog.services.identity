using MediatR;
using riverli.blog.services.identity.Application.Interfaces;

namespace riverli.blog.services.identity.Application.Features.Roles.Commands;

public class UpdateRoleMenusCommandHandler : IRequestHandler<UpdateRoleMenusCommand, bool>
{
    private readonly IRoleRepository _roleRepository;

    public UpdateRoleMenusCommandHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<bool> Handle(UpdateRoleMenusCommand request, CancellationToken cancellationToken)
    {
        await _roleRepository.UpdateRoleMenusAsync(request.RoleId, request.MenuIds, cancellationToken);
        return true;
    }
}
