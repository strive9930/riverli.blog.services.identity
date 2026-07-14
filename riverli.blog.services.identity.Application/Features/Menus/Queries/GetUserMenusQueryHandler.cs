using MediatR;
using riverli.blog.services.identity.Application.DTOs;
using riverli.blog.services.identity.Application.Interfaces;

namespace riverli.blog.services.identity.Application.Features.Menus.Queries;

public class GetUserMenusQueryHandler : IRequestHandler<GetUserMenusQuery, List<MenuDto>>
{
    private readonly IMenuRepository _menuRepository;

    // 注入接口，而不是 DbContext
    public GetUserMenusQueryHandler(IMenuRepository menuRepository)
    {
        _menuRepository = menuRepository;
    }

    public async Task<List<MenuDto>> Handle(GetUserMenusQuery request, CancellationToken cancellationToken)
    {
        // 具体的查询逻辑下放给 Infrastructure 层去实现
        return await _menuRepository.GetUserMenusAsync(request.UserId, cancellationToken);
    }
}