using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Application.Users.Queries.GetAllUsers;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Users.Queries.GetUserRoles;

/// <summary>
/// 获取用户角色列表查询处理器
/// </summary>
public class GetUserRolesQueryHandler : IRequestHandler<GetUserRolesQuery, Result<List<RoleDto>>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IdentityServiceDbContext _context;

    public GetUserRolesQueryHandler(
        UserManager<ApplicationUser> userManager,
        IdentityServiceDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<Result<List<RoleDto>>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
            {
                return Result<List<RoleDto>>.FailResult("用户不存在");
            }

            var roles = await _userManager.GetRolesAsync(user);
            
            // 根据角色名称查询对应的角色实体，获取真实的角色 ID
            var roleDtos = await _context.Roles
                .Where(r => roles.Contains(r.Name!))
                .Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name!,
                    Description = r.Description ?? $"{r.Name}角色"
                })
                .ToListAsync(cancellationToken);

            return Result<List<RoleDto>>.SuccessResult(roleDtos);
        }
        catch (Exception ex)
        {
            return Result<List<RoleDto>>.FailResult($"获取用户角色失败：{ex.Message}");
        }
    }
}
