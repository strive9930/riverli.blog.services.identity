using MediatR;
using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Roles.Queries
{
    // 用于SQL查询结果映射的内部类
    internal class RoleUserCount
    {
        public Guid RoleId { get; set; }
        public int UserCount { get; set; }
    }

    /// <summary>
    /// 获取所有角色查询处理器
    /// </summary>
    public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, Result<List<RoleDto>>>
    {
        private readonly IdentityServiceDbContext _context;
    
        public GetAllRolesQueryHandler(IdentityServiceDbContext context)
        {
            _context = context;
        }
    
        public async Task<Result<List<RoleDto>>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var query = _context.Roles
                    .Include(r => r.Permissions)
                    .AsQueryable();
    
                // 根据启用状态筛选
                if (request.IsEnabled.HasValue)
                {
                    query = query.Where(r => r.IsEnabled == request.IsEnabled.Value);
                }
    
                // 根据关键词筛选
                if (!string.IsNullOrEmpty(request.Keyword))
                {
                    var keyword = request.Keyword.ToLower();
                    query = query.Where(r => 
                        r.Name!.ToLower().Contains(keyword) || 
                        r.Code.ToLower().Contains(keyword) ||
                        (r.Description != null && r.Description.ToLower().Contains(keyword)));
                }
    
                var roles = await query
                    .OrderBy(r => r.Name)
                    .ToListAsync(cancellationToken);
    
                // 优化：使用原生 SQL 查询获取角色用户数量，避免实体映射问题
                var roleUserCounts = await _context.Database
                    .SqlQueryRaw<RoleUserCount>(@"
                        SELECT RoleId, COUNT(UserId) as UserCount
                        FROM AspNetUserRoles
                        GROUP BY RoleId")
                    .ToDictionaryAsync(x => x.RoleId, x => x.UserCount, cancellationToken);
    
                var result = roles.Select(role => new RoleDto
                {
                    Id = role.Id,
                    Name = role.Name ?? string.Empty,
                    Code = role.Code,
                    Description = role.Description,
                    IsEnabled = role.IsEnabled,
                    UserCount = roleUserCounts.GetValueOrDefault(role.Id, 0),
                    PermissionCount = role.Permissions.Count,
                    CreateTime = role.CreateTime,
                    UpdateTime = role.UpdateTime
                }).ToList();
    
                return Result<List<RoleDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return Result<List<RoleDto>>.FailResult($"获取角色列表失败：{ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// 分页获取角色列表查询处理器
    /// </summary>
    public class GetPagedRolesQueryHandler : IRequestHandler<GetPagedRolesQuery, PagedResult<RoleDto>>
    {
        private readonly IdentityServiceDbContext _context;
    
        public GetPagedRolesQueryHandler(IdentityServiceDbContext context)
        {
            _context = context;
        }
    
        public async Task<PagedResult<RoleDto>> Handle(GetPagedRolesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var query = _context.Roles
                    .Include(r => r.Permissions)
                    .AsQueryable();
    
                // 根据启用状态筛选
                if (request.IsEnabled.HasValue)
                {
                    query = query.Where(r => r.IsEnabled == request.IsEnabled.Value);
                }
    
                // 根据关键词筛选
                if (!string.IsNullOrEmpty(request.Keyword))
                {
                    var keyword = request.Keyword.ToLower();
                    query = query.Where(r =>
                        r.Name!.ToLower().Contains(keyword) ||
                        r.Code.ToLower().Contains(keyword) ||
                        (r.Description != null && r.Description.ToLower().Contains(keyword)));
                }
    
                // 排序
                query = request.SortBy?.ToLower() switch
                {
                    "name" => request.SortDesc ? query.OrderByDescending(r => r.Name) : query.OrderBy(r => r.Name),
                    "code" => request.SortDesc ? query.OrderByDescending(r => r.Code) : query.OrderBy(r => r.Code),
                    "createtime" => request.SortDesc ? query.OrderByDescending(r => r.CreateTime) : query.OrderBy(r => r.CreateTime),
                    "updatetime" => request.SortDesc ? query.OrderByDescending(r => r.UpdateTime) : query.OrderBy(r => r.UpdateTime),
                    _ => request.SortDesc ? query.OrderByDescending(r => r.Name) : query.OrderBy(r => r.Name)
                };
    
                // 分页
                var totalCount = await query.CountAsync(cancellationToken);
                var roles = await query
                    .Skip((request.PageIndex - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync(cancellationToken);
    
                // 优化：使用原生 SQL 查询获取角色用户数量，避免实体映射问题
                var roleUserCounts = await _context.Database
                    .SqlQueryRaw<RoleUserCount>(@"
                        SELECT RoleId, COUNT(UserId) as UserCount
                        FROM AspNetUserRoles
                        GROUP BY RoleId")
                    .ToDictionaryAsync(x => x.RoleId, x => x.UserCount, cancellationToken);
    
                var result = roles.Select(role => new RoleDto
                {
                    Id = role.Id,
                    Name = role.Name ?? string.Empty,
                    Code = role.Code,
                    Description = role.Description,
                    IsEnabled = role.IsEnabled,
                    UserCount = roleUserCounts.GetValueOrDefault(role.Id, 0),
                    PermissionCount = role.Permissions.Count,
                    CreateTime = role.CreateTime,
                    UpdateTime = role.UpdateTime
                }).ToList();
    
                return PagedResult<RoleDto>.SuccessResult(result, totalCount, request.PageIndex, request.PageSize);
            }
            catch (Exception ex)
            {
                return PagedResult<RoleDto>.FailResult($"分页获取角色列表失败：{ex.Message}");
            }
        }
    }

    /// <summary>
    /// 获取角色详情查询处理器
    /// </summary>
    public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, Result<RoleDetailDto>>
    {
        private readonly IdentityServiceDbContext _context;

        public GetRoleByIdQueryHandler(IdentityServiceDbContext context)
        {
            _context = context;
        }

        public async Task<Result<RoleDetailDto>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var role = await _context.Roles
                    .Include(r => r.Permissions)
                    .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

                if (role == null)
                {
                    return Result<RoleDetailDto>.FailResult("角色不存在");
                }

                // 查询该角色的用户信息
                var (userCount, users) = await GetRoleUsersInfo(role.Id, cancellationToken);

                var result = new RoleDetailDto
                {
                    Id = role.Id,
                    Name = role.Name ?? string.Empty,
                    Code = role.Code,
                    Description = role.Description,
                    IsEnabled = role.IsEnabled,
                    UserCount = userCount,
                    PermissionCount = role.Permissions.Count,
                    CreateTime = role.CreateTime,
                    UpdateTime = role.UpdateTime,
                    Permissions = role.Permissions.Select(p => new PermissionDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Code = p.Code,
                        Description = p.Description,
                        Group = p.Group
                    }).ToList(),
                    Users = users
                };

                return Result<RoleDetailDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return Result<RoleDetailDto>.FailResult($"获取角色详情失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取角色用户信息的辅助方法
        /// </summary>
        private async Task<(int userCount, List<UserDto> users)> GetRoleUsersInfo(Guid roleId, CancellationToken cancellationToken)
        {
            // 使用原生SQL查询获取角色用户信息，避免实体映射问题
            var userIds = await _context.Database
                .SqlQueryRaw<Guid>(@"
                    SELECT UserId 
                    FROM AspNetUserRoles 
                    WHERE RoleId = {0}", roleId)
                .ToListAsync(cancellationToken);

            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    UserName = u.UserName ?? string.Empty,
                    Email = u.Email,
                    NickName = u.NickName
                })
                .ToListAsync(cancellationToken);

            return (users.Count, users);
        }
    }
}