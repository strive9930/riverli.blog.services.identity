using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Application.Users.Commands.User;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Users.Commands.User;

#region 用户查询处理器

/// <summary>
/// 获取所有用户的查询处理器
/// </summary>
public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, PagedResult<UserDto>>
{
    private readonly IdentityServiceDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public GetAllUsersQueryHandler(
        IdentityServiceDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<PagedResult<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
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
                // 注意：ApplicationUser 没有 IsEnabled 字段，这里可能需要根据实际需求调整
                // 暂时忽略此筛选条件
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
                
                // 根据角色名称查询对应的角色实体，获取真实的角色ID
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
                    LastLoginTime = user.AccessFailedCount > 0 ? DateTime.UtcNow : (DateTime?)null, // 简单模拟
                    Roles = roleDtos
                });
            }

            // 使用DDD核心库的标准PagedResult
            var pagedQuery = new PagedQuery
            {
                PageIndex = request.PageIndex,
                PageSize = request.PageSize
            };
            
            return PagedResult<UserDto>.Create(pagedQuery, 
                totalCount, 
                userDtos);
        }
        catch (Exception ex)
        {
            var pagedQuery = new PagedQuery
            {
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
            };
            Console.WriteLine("获取所有用户的查询处理器异常：",ex.Message+ex.StackTrace);
            return PagedResult<UserDto>.Empty(pagedQuery);
        }
    }
}

/// <summary>
/// 获取用户角色的查询处理器
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
            
            // 根据角色名称查询对应的角色实体，获取真实的角色ID
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
            return Result<List<RoleDto>>.FailResult($"获取用户角色失败: {ex.Message}");
        }
    }
}

#endregion