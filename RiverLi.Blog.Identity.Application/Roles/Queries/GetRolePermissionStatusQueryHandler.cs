using MediatR;
using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Roles.Queries;

/// <summary>
/// 获取角色权限分配状态查询
/// </summary>
public record GetRolePermissionStatusQuery(Guid RoleId) : IRequest<Result<RolePermissionStatusDto>>;

/// <summary>
/// 角色权限状态 DTO
/// </summary>
public class RolePermissionStatusDto
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public List<PermissionItemDto> AssignedPermissions { get; set; } = new();
    public List<PermissionItemDto> UnassignedPermissions { get; set; } = new();
}

/// <summary>
/// 权限项 DTO
/// </summary>
public class PermissionItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsAssigned { get; set; }
}

/// <summary>
/// 获取角色权限分配状态查询处理器
/// </summary>
public class GetRolePermissionStatusQueryHandler : IRequestHandler<GetRolePermissionStatusQuery, Result<RolePermissionStatusDto>>
{
    private readonly IdentityServiceDbContext _context;

    public GetRolePermissionStatusQueryHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<RolePermissionStatusDto>> Handle(GetRolePermissionStatusQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // 查找角色并加载已分配的权限
            var role = await _context.Roles
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

            if (role == null)
            {
                return Result<RolePermissionStatusDto>.FailResult("角色不存在");
            }

            // 获取所有权限
            var allPermissions = await _context.Permissions
                .ToListAsync(cancellationToken);

            // 已分配的权限 ID
            var assignedPermissionIds = role.Permissions.Select(p => p.Id).ToHashSet();

            // 构建已分配权限列表
            var assignedPermissions = allPermissions
                .Where(p => assignedPermissionIds.Contains(p.Id))
                .Select(p => new PermissionItemDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Code = p.Code,
                    Group = p.Group,
                    Description = p.Description,
                    IsAssigned = true
                })
                .ToList();

            // 构建未分配权限列表
            var unassignedPermissions = allPermissions
                .Where(p => !assignedPermissionIds.Contains(p.Id))
                .Select(p => new PermissionItemDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Code = p.Code,
                    Group = p.Group,
                    Description = p.Description,
                    IsAssigned = false
                })
                .ToList();

            var result = new RolePermissionStatusDto
            {
                RoleId = role.Id,
                RoleName = role.Name ?? string.Empty,
                AssignedPermissions = assignedPermissions,
                UnassignedPermissions = unassignedPermissions
            };

            return Result<RolePermissionStatusDto>.SuccessResult(result);
        }
        catch (Exception ex)
        {
            return Result<RolePermissionStatusDto>.FailResult($"获取权限状态失败：{ex.Message}");
        }
    }
}
