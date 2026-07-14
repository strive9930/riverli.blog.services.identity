using MediatR;
using riverli.blog.services.identity.Application.Interfaces;

namespace riverli.blog.services.identity.Application.Features.Roles.Queries;

public class GetRoleApisQueryHandler : IRequestHandler<GetRoleApisQuery, List<Guid>>
{
    private readonly IRoleRepository _roleRepository;

    public GetRoleApisQueryHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<List<Guid>> Handle(GetRoleApisQuery request, CancellationToken cancellationToken)
    {
        return await _roleRepository.GetRoleApiIdsAsync(request.RoleId, cancellationToken);
    }
}
