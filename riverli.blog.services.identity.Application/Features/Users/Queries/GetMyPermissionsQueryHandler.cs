using MediatR;
using riverli.blog.services.identity.Application.Interfaces;

namespace riverli.blog.services.identity.Application.Features.Users.Queries;

/// <summary>
/// 处理 GetMyPermissionsQuery：从仓储获取当前用户拥有的所有 API 权限标识
/// </summary>
public class GetMyPermissionsQueryHandler : IRequestHandler<GetMyPermissionsQuery, List<string>>
{
    private readonly IMenuRepository _menuRepository;

    public GetMyPermissionsQueryHandler(IMenuRepository menuRepository)
    {
        _menuRepository = menuRepository;
    }

    public async Task<List<string>> Handle(GetMyPermissionsQuery request, CancellationToken cancellationToken)
    {
        return await _menuRepository.GetUserPermissionsAsync(request.UserId, cancellationToken);
    }
}
