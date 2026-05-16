using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Application.Users.Queries.GetAllUsers;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Users.Queries.GetUserById;

/// <summary>
/// 根据用户 ID 获取用户详情查询处理器
/// </summary>
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDetailDto>>
{
    private readonly IdentityServiceDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public GetUserByIdQueryHandler(
        IdentityServiceDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<Result<UserDetailDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                return Result<UserDetailDto>.FailResult("用户不存在");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var roleDtos = await _context.Roles
                .Where(r => roles.Contains(r.Name!))
                .Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name!,
                    Description = r.Description
                })
                .ToListAsync(cancellationToken);

            var dto = new UserDetailDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                NickName = user.NickName,
                PhoneNumber = user.PhoneNumber,
                IsEnabled = user.IsEnabled,
                CreateTime = user.CreateTime,
                UpdateTime = user.UpdateTime,
                Roles = roleDtos
            };

            return Result<UserDetailDto>.SuccessResult(dto);
        }
        catch (Exception ex)
        {
            return Result<UserDetailDto>.FailResult($"获取用户详情失败：{ex.Message}");
        }
    }
}
