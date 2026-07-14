using MediatR;
using riverli.blog.services.identity.Application.Interfaces;

namespace riverli.blog.services.identity.Application.Features.Roles.Queries;

public class GetRoleMenusQueryHandler : IRequestHandler<GetRoleMenusQuery, List<Guid>>
{
    private readonly IRoleRepository _roleRepository;

    public GetRoleMenusQueryHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<List<Guid>> Handle(GetRoleMenusQuery request, CancellationToken cancellationToken)
    {
        return await _roleRepository.GetRoleMenuIdsAsync(request.RoleId, cancellationToken);
    }
}
