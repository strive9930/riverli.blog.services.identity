using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Users.Queries.GetAllUsers;

/// <summary>
/// 获取所有用户查询处理器（分页）
/// </summary>
public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, PagedResult<UserDto>>
{
    private readonly IdentityServiceDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<GetAllUsersQueryHandler> _logger;

    public GetAllUsersQueryHandler(
        IdentityServiceDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<GetAllUsersQueryHandler> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<PagedResult<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("查询用户列表，页码：{PageIndex}, 每页数量：{PageSize}, 关键字：{Keyword}", 
                request.PageIndex, request.PageSize, request.Keyword ?? "无");
            
            var query = _context.Users.AsQueryable();

            // 关键字搜索
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.ToLower();
                query = query.Where(u => 
                    u.UserName!.ToLower().Contains(keyword) ||
                    u.Email!.ToLower().Contains(keyword) ||
                    (u.NickName != null && u.NickName.ToLower().Contains(keyword)));
            }

            // 状态筛选
            if (request.IsEnabled.HasValue)
            {
                query = query.Where(u => u.IsEnabled == request.IsEnabled.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);
            
            var users = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // 获取每个用户的角色信息
            var userDtos = new List<UserDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                
                // 根据角色名称查询对应的角色实体，获取真实的角色 ID
                var roleDtos = await _context.Roles
                    .Where(r => roles.Contains(r.Name!))
                    .Select(r => new RoleDto
                    {
                        Id = r.Id,
                        Name = r.Name!,
                        Description = r.Description
                    })
                    .ToListAsync(cancellationToken);

                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    NickName = user.NickName,
                    PhoneNumber = user.PhoneNumber,
                    IsEnabled = user.IsEnabled, 
                    CreateTime = user.CreateTime,
                    UpdateTime = user.UpdateTime,
                    LastLoginTime = null, // ApplicationUser 中没有这个字段
                    Roles = roleDtos
                });
            }

            /*var pagedResult = PagedResult<UserDto>.Create(
                new PagedQuery { PageIndex = request.PageIndex, PageSize = request.PageSize },
                totalCount, 
                userDtos);*/
            
            _logger.LogInformation("成功获取用户列表，总数：{Total}, 返回：{Count}", totalCount, userDtos.Count);
            return PagedResult<UserDto>.SuccessResult(userDtos,totalCount,request.PageIndex,request.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查询用户列表失败");
            return PagedResult<UserDto>.FailResult($"查询失败：{ex.Message}");
        }
    }
}
