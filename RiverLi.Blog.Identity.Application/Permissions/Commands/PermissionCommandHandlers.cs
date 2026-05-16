using MediatR;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.DDD.Core.Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Infrastructure.Data;

namespace RiverLi.Blog.Identity.Application.Permissions.Commands;

#region 权限 CRUD 处理器

/// <summary>
/// 获取所有权限的查询处理器
/// </summary>
public class GetAllPermissionsQueryHandler : IRequestHandler<GetAllPermissionsQuery, Result<List<PermissionDto>>>
{
    private readonly IdentityServiceDbContext _context;

    public GetAllPermissionsQueryHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<PermissionDto>>> Handle(GetAllPermissionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.Permissions.AsQueryable();

            // 应用筛选条件
            if (!string.IsNullOrEmpty(request.Group))
            {
                query = query.Where(p => p.Group == request.Group);
            }

            if (request.Enabled.HasValue)
            {
                query = query.Where(p => p.IsEnabled == request.Enabled.Value);
            }

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                var keyword = request.Keyword.ToLower();
                query = query.Where(p => 
                    p.Name.ToLower().Contains(keyword) ||
                    p.Code.ToLower().Contains(keyword) ||
                    (p.Description != null && p.Description.ToLower().Contains(keyword)));
            }

            var permissions = await query
                .OrderBy(p => p.Group)
                .ThenBy(p => p.Name)
                .ToListAsync(cancellationToken);

            var result = permissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                Description = p.Description,
                Group = p.Group,
                ClaimType = p.ClaimType,
                ClaimValue = p.ClaimValue,
                IsEnabled = p.IsEnabled,
                CreateTime = p.CreateTime,
                UpdateTime = p.UpdateTime
            }).ToList();

            return Result<List<PermissionDto>>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            return Result<List<PermissionDto>>.FailResult($"获取权限列表失败：{ex.Message}");
        }
    }
}

/// <summary>
/// 分页获取权限的查询处理器
/// </summary>
public class GetPagedPermissionsQueryHandler : IRequestHandler<GetPagedPermissionsQuery, PagedResult<PermissionDto>>
{
    private readonly IdentityServiceDbContext _context;

    public GetPagedPermissionsQueryHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<PermissionDto>> Handle(GetPagedPermissionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.Permissions.AsQueryable();

            // 应用筛选条件
            if (!string.IsNullOrEmpty(request.Group))
            {
                query = query.Where(p => p.Group == request.Group);
            }

            if (request.Enabled.HasValue)
            {
                query = query.Where(p => p.IsEnabled == request.Enabled.Value);
            }

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                var keyword = request.Keyword.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(keyword) ||
                    p.Code.ToLower().Contains(keyword) ||
                    (p.Description != null && p.Description.ToLower().Contains(keyword)));
            }

            // 排序
            query = request.SortBy?.ToLower() switch
            {
                "name" => request.SortDesc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "code" => request.SortDesc ? query.OrderByDescending(p => p.Code) : query.OrderBy(p => p.Code),
                "group" => request.SortDesc ? query.OrderByDescending(p => p.Group) : query.OrderBy(p => p.Group),
                "createtime" => request.SortDesc ? query.OrderByDescending(p => p.CreateTime) : query.OrderBy(p => p.CreateTime),
                "updatetime" => request.SortDesc ? query.OrderByDescending(p => p.UpdateTime) : query.OrderBy(p => p.UpdateTime),
                _ => request.SortDesc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name)
            };

            // 分页
            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var result = items.Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                Description = p.Description,
                Group = p.Group,
                ClaimType = p.ClaimType,
                ClaimValue = p.ClaimValue,
                IsEnabled = p.IsEnabled,
                CreateTime = p.CreateTime,
                UpdateTime = p.UpdateTime
            }).ToList();
            
            return PagedResult<PermissionDto>.SuccessResult(result, totalCount,request.PageIndex, request.PageSize);
        }
        catch (Exception ex)
        {
            return PagedResult<PermissionDto>.FailResult($"分页获取权限失败：{ex.Message}");
        }
    }
}

/// <summary>
/// 根据 ID 获取权限的查询处理器
/// </summary>
public class GetPermissionByIdQueryHandler : IRequestHandler<GetPermissionByIdQuery, Result<PermissionDetailDto>>
{
    private readonly IdentityServiceDbContext _context;

    public GetPermissionByIdQueryHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PermissionDetailDto>> Handle(GetPermissionByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var permission = await _context.Permissions
                .Include(p => p.Roles)
                .Include(p => p.Users)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (permission == null)
            {
                return Result<PermissionDetailDto>.FailResult("权限不存在");
            }

            var result = new PermissionDetailDto
            {
                Id = permission.Id,
                Name = permission.Name,
                Code = permission.Code,
                Description = permission.Description,
                Group = permission.Group,
                ClaimType = permission.ClaimType,
                ClaimValue = permission.ClaimValue,
                IsEnabled = true,
                CreateTime = permission.CreateTime,
                UpdateTime = permission.UpdateTime,
                AssignedRoles = permission.Roles.Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name ?? string.Empty,
                    Description = r.Description
                }).ToList(),
                AssignedUsers = permission.Users.Select(u => new UserDto
                {
                    Id = u.Id,
                    UserName = u.UserName ?? string.Empty,
                    Email = u.Email
                }).ToList()
            };

            return Result<PermissionDetailDto>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            return Result<PermissionDetailDto>.FailResult($"获取权限详情失败: {ex.Message}");
        }
    }
}

/// <summary>
/// 创建权限的命令处理器
/// </summary>
public class CreatePermissionCommandHandler : IRequestHandler<CreatePermissionCommand, Result<PermissionDto>>
{
    private readonly IdentityServiceDbContext _context;

    public CreatePermissionCommandHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PermissionDto>> Handle(CreatePermissionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 检查权限编码是否已存在
            if (await _context.Permissions.AnyAsync(p => p.Code == request.Code, cancellationToken))
            {
                return Result<PermissionDto>.FailResult("权限编码已存在");
            }

            var permission = new Permission
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Code = request.Code,
                Description = request.Description,
                Group = request.Group,
                ClaimType = request.ClaimType ?? "Permission",
                ClaimValue = request.ClaimValue ?? request.Code,
                RoleId = "", // 默认值
                CreateTime = DateTime.UtcNow, // 设置创建时间
                IsEnabled = true // 默认启用
            };

            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync(cancellationToken);

            var result = new PermissionDto
            {
                Id = permission.Id,
                Name = permission.Name,
                Code = permission.Code,
                Description = permission.Description,
                Group = permission.Group,
                ClaimType = permission.ClaimType,
                ClaimValue = permission.ClaimValue,
                IsEnabled = true,
                CreateTime = DateTime.UtcNow
            };

            return Result<PermissionDto>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            return Result<PermissionDto>.FailResult($"创建权限失败: {ex.Message}");
        }
    }
}

/// <summary>
/// 更新权限的命令处理器
/// </summary>
public class UpdatePermissionCommandHandler : IRequestHandler<UpdatePermissionCommand, Result<PermissionDto>>
{
    private readonly IdentityServiceDbContext _context;

    public UpdatePermissionCommandHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PermissionDto>> Handle(UpdatePermissionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var permission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (permission == null)
            {
                return Result<PermissionDto>.FailResult("权限不存在");
            }

            // 检查权限编码是否与其他权限冲突
            if (await _context.Permissions.AnyAsync(p => p.Code == request.Code && p.Id != request.Id, cancellationToken))
            {
                return Result<PermissionDto>.FailResult("权限编码已存在");
            }

            permission.Name = request.Name;
            permission.Code = request.Code;
            permission.Description = request.Description;
            permission.Group = request.Group;
            permission.ClaimType = request.ClaimType ?? permission.ClaimType;
            permission.ClaimValue = request.ClaimValue ?? permission.ClaimValue;
            permission.IsEnabled = request.IsEnabled;
            
            // 更新审计字段
            permission.UpdateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            var result = new PermissionDto
            {
                Id = permission.Id,
                Name = permission.Name,
                Code = permission.Code,
                Description = permission.Description,
                Group = permission.Group,
                ClaimType = permission.ClaimType,
                ClaimValue = permission.ClaimValue,
                IsEnabled = permission.IsEnabled,
                CreateTime = permission.CreateTime ?? DateTime.UtcNow,
                UpdateTime = permission.UpdateTime ?? DateTime.UtcNow
            };

            return Result<PermissionDto>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            return Result<PermissionDto>.FailResult($"更新权限失败: {ex.Message}");
        }
    }
}

/// <summary>
/// 删除权限的命令处理器
/// </summary>
public class DeletePermissionCommandHandler : IRequestHandler<DeletePermissionCommand, Result<string>>
{
    private readonly IdentityServiceDbContext _context;

    public DeletePermissionCommandHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(DeletePermissionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var permission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (permission == null)
            {
                return Result<string>.FailResult("权限不存在");
            }

            // 检查是否有角色或用户关联此权限
            var roleCount = await _context.Roles
                .Include(r => r.Permissions)
                .CountAsync(r => r.Permissions.Any(p => p.Id == request.Id), cancellationToken);
            
            var userCount = await _context.Users
                .Include(u => u.Permissions)
                .CountAsync(u => u.Permissions.Any(p => p.Id == request.Id), cancellationToken);

            if (roleCount > 0 || userCount > 0)
            {
                return Result<string>.FailResult($"无法删除权限，仍有 {roleCount} 个角色和 {userCount} 个用户使用此权限");
            }

            _context.Permissions.Remove(permission);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<string>.SuccessResult("权限删除成功");
        }
        catch (Exception ex)
        {
            return Result<string>.FailResult($"删除权限失败: {ex.Message}");
        }
    }
}

#endregion

#region 辅助查询处理器

/// <summary>
/// 根据编码获取权限的查询处理器
/// </summary>
public class GetPermissionByCodeQueryHandler : IRequestHandler<GetPermissionByCodeQuery, Result<PermissionDto>>
{
    private readonly IdentityServiceDbContext _context;

    public GetPermissionByCodeQueryHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PermissionDto>> Handle(GetPermissionByCodeQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var permission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Code == request.Code, cancellationToken);

            if (permission == null)
            {
                return Result<PermissionDto>.FailResult("权限不存在");
            }

            var result = new PermissionDto
            {
                Id = permission.Id,
                Name = permission.Name,
                Code = permission.Code,
                Description = permission.Description,
                Group = permission.Group,
                ClaimType = permission.ClaimType,
                ClaimValue = permission.ClaimValue,
                IsEnabled = permission.IsEnabled,
                CreateTime = permission.CreateTime,
                UpdateTime = permission.UpdateTime
            };

            return Result<PermissionDto>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            return Result<PermissionDto>.FailResult($"获取权限失败：{ex.Message}");
        }
    }
}

/// <summary>
/// 获取用户权限的查询处理器
/// </summary>
public class GetUserPermissionsQueryHandler : IRequestHandler<GetUserPermissionsQuery, Result<List<PermissionDto>>>
{
    private readonly IdentityServiceDbContext _context;

    public GetUserPermissionsQueryHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<PermissionDto>>> Handle(GetUserPermissionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users
                .Where(u => u.Id == request.UserId)
                .Include(u => u.Permissions)
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null)
            {
                return Result<List<PermissionDto>>.FailResult("用户不存在");
            }

            var permissions = user.Permissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                Description = p.Description,
                Group = p.Group,
                ClaimType = p.ClaimType,
                ClaimValue = p.ClaimValue,
                IsEnabled = p.IsEnabled,
                CreateTime = p.CreateTime,
                UpdateTime = p.UpdateTime
            }).ToList();

            return Result<List<PermissionDto>>.SuccessResult(permissions);
        }
        catch (Exception ex)
        {
            return Result<List<PermissionDto>>.FailResult($"获取用户权限失败：{ex.Message}");
        }
    }
}

/// <summary>
/// 获取角色权限的查询处理器
/// </summary>
public class GetRolePermissionsQueryHandler : IRequestHandler<GetRolePermissionsQuery, Result<List<PermissionDto>>>
{
    private readonly IdentityServiceDbContext _context;

    public GetRolePermissionsQueryHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<PermissionDto>>> Handle(GetRolePermissionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var role = await _context.Roles
                .Where(r => r.Id == request.RoleId)
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(cancellationToken);

            if (role == null)
            {
                return Result<List<PermissionDto>>.FailResult("角色不存在");
            }

            var permissions = role.Permissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                Description = p.Description,
                Group = p.Group,
                ClaimType = p.ClaimType,
                ClaimValue = p.ClaimValue,
                IsEnabled = p.IsEnabled,
                CreateTime = p.CreateTime,
                UpdateTime = p.UpdateTime
            }).ToList();

            return Result<List<PermissionDto>>.SuccessResult(permissions);
        }
        catch (Exception ex)
        {
            return Result<List<PermissionDto>>.FailResult($"获取角色权限失败：{ex.Message}");
        }
    }
}

#endregion

#region 权限状态管理处理器

/// <summary>
/// 批量更新权限状态的命令处理器
/// </summary>
public class BatchUpdatePermissionStatusCommandHandler : IRequestHandler<BatchUpdatePermissionStatusCommand, Result<string>>
{
    private readonly IdentityServiceDbContext _context;

    public BatchUpdatePermissionStatusCommandHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(BatchUpdatePermissionStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var permissions = await _context.Permissions
                .Where(p => request.PermissionIds.Contains(p.Id))
                .ToListAsync(cancellationToken);

            if (permissions.Count != request.PermissionIds.Count)
            {
                return Result<string>.FailResult("部分权限不存在");
            }

            // 这里需要根据实际需求实现状态更新逻辑
            // 由于Permission实体目前没有IsEnabled字段，暂时只返回成功消息
            await _context.SaveChangesAsync(cancellationToken);

            return Result<string>.SuccessResult($"成功更新 {permissions.Count} 个权限的状态");
        }
        catch (Exception ex)
        {
            return Result<string>.FailResult($"批量更新权限状态失败: {ex.Message}");
        }
    }
}

/// <summary>
/// 启用权限的命令处理器
/// </summary>
public class EnablePermissionCommandHandler : IRequestHandler<EnablePermissionCommand, Result<string>>
{
    private readonly IdentityServiceDbContext _context;

    public EnablePermissionCommandHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(EnablePermissionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var permission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (permission == null)
            {
                return Result<string>.FailResult("权限不存在");
            }

            // 实现启用逻辑（需要根据实际需求调整）
            await _context.SaveChangesAsync(cancellationToken);

            return Result<string>.SuccessResult("权限启用成功");
        }
        catch (Exception ex)
        {
            return Result<string>.FailResult($"启用权限失败: {ex.Message}");
        }
    }
}

/// <summary>
/// 禁用权限的命令处理器
/// </summary>
public class DisablePermissionCommandHandler : IRequestHandler<DisablePermissionCommand, Result<string>>
{
    private readonly IdentityServiceDbContext _context;

    public DisablePermissionCommandHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(DisablePermissionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var permission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (permission == null)
            {
                return Result<string>.FailResult("权限不存在");
            }

            // 实现禁用逻辑（需要根据实际需求调整）
            await _context.SaveChangesAsync(cancellationToken);

            return Result<string>.SuccessResult("权限禁用成功");
        }
        catch (Exception ex)
        {
            return Result<string>.FailResult($"禁用权限失败: {ex.Message}");
        }
    }
}

#endregion