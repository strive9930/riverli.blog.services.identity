using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Auth.Queries.GetCurrentUserInfo;

/// <summary>
/// 获取当前用户信息查询处理器
/// </summary>
public class GetCurrentUserInfoQueryHandler : IRequestHandler<GetCurrentUserInfoQuery, Result<UserInfoDto>>
{
    private readonly IdentityServiceDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<GetCurrentUserInfoQueryHandler> _logger;

    public GetCurrentUserInfoQueryHandler(
        IdentityServiceDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<GetCurrentUserInfoQueryHandler> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result<UserInfoDto>> Handle(GetCurrentUserInfoQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("查询用户详情，用户 ID: {UserId}", request.UserId);
            
            // 获取用户基本信息
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("用户不存在，ID: {UserId}", request.UserId);
                return Result<UserInfoDto>.FailResult("用户不存在");
            }

            // 通过 UserManager 获取用户的角色
            var roles = await _userManager.GetRolesAsync(user);
            var roleNames = roles.ToList();

            // 获取角色对应的权限
            var rolePermissions = await _context.Roles
                .Include(r => r.Permissions)
                .Where(r => roleNames.Contains(r.Name))
                .SelectMany(r => r.Permissions)
                .Select(p => p.Code)
                .Distinct()
                .ToListAsync(cancellationToken);

            var userInfo = new UserInfoDto
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                NickName = user.NickName,
                Email = user.Email,
                Avatar = user.AvatarUrl,
                Roles = roleNames,
                Permissions = rolePermissions,
                IsAdmin = roleNames.Contains("Admin") || roleNames.Contains("Administrator"),
                LastLoginTime = null, // ApplicationUser 中没有这个字段
                CreatedAt = user.CreateTime
            };

            _logger.LogInformation("成功获取用户详情，用户 ID: {UserId}", request.UserId);
            return Result<UserInfoDto>.SuccessResult(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户详情失败，用户 ID: {UserId}", request.UserId);
            return Result<UserInfoDto>.FailResult($"获取用户信息失败：{ex.Message}");
        }
    }
}
