using MediatR;
using Microsoft.AspNetCore.Identity;
using riverli.blog.services.identity.Domain.Entities;

namespace riverli.blog.services.identity.Application.Features.Users.Queries;

/// <summary>
/// 查询用户角色处理器：通过 UserManager 获取用户绑定的角色名称列表
/// </summary>
public class GetUserRolesQueryHandler : IRequestHandler<GetUserRolesQuery, List<string>>
{
    private readonly UserManager<AppUser> _userManager;

    public GetUserRolesQueryHandler(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<List<string>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            throw new KeyNotFoundException($"用户不存在: {request.UserId}");
        }

        var roles = await _userManager.GetRolesAsync(user);
        return roles.ToList();
    }
}
