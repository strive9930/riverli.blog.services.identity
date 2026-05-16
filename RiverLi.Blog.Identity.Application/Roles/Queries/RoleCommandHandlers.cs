using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Roles.Queries;

/// <summary>
/// 创建角色命令处理器
/// </summary>
public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Result<RoleDto>>
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IdentityServiceDbContext _context;

    public CreateRoleCommandHandler(
        RoleManager<ApplicationRole> roleManager,
        IdentityServiceDbContext context)
    {
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<Result<RoleDto>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 检查角色名称是否已存在
            var existingRoleByName = await _roleManager.FindByNameAsync(request.Name);
            if (existingRoleByName != null)
            {
                return Result<RoleDto>.FailResult("角色名称已存在");
            }

            // 检查角色编码是否已存在
            var existingRoleByCode = await _context.Roles
                .FirstOrDefaultAsync(r => r.Code == request.Code, cancellationToken);
            if (existingRoleByCode != null)
            {
                return Result<RoleDto>.FailResult("角色编码已存在");
            }

            // 创建新角色
            var role = new ApplicationRole
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Code = request.Code,
                Description = request.Description,
                IsEnabled = request.IsEnabled,
                CreateTime = DateTime.UtcNow
            };

            var result = await _roleManager.CreateAsync(role);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<RoleDto>.FailResult($"创建角色失败: {errors}");
            }

            var roleDto = new RoleDto
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty,
                Code = role.Code,
                Description = role.Description,
                IsEnabled = role.IsEnabled,
                UserCount = 0,
                PermissionCount = 0,
                CreateTime = role.CreateTime,
                UpdateTime = role.UpdateTime
            };

            return Result<RoleDto>.SuccessResult(roleDto, "角色创建成功");
        }
        catch (Exception ex)
        {
            return Result<RoleDto>.FailResult($"创建角色时发生错误: {ex.Message}");
        }
    }
}

/// <summary>
/// 更新角色命令处理器
/// </summary>
public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, Result<bool>>
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IdentityServiceDbContext _context;

    public UpdateRoleCommandHandler(
        RoleManager<ApplicationRole> roleManager,
        IdentityServiceDbContext context)
    {
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<Result<bool>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var role = await _roleManager.FindByIdAsync(request.Id.ToString());
            if (role == null)
            {
                return Result<bool>.FailResult("角色不存在");
            }

            // 检查名称唯一性（如果要更新名称）
            if (!string.IsNullOrEmpty(request.Name) && request.Name != role.Name)
            {
                var existingRole = await _roleManager.FindByNameAsync(request.Name);
                if (existingRole != null)
                {
                    return Result<bool>.FailResult("角色名称已存在");
                }
                role.Name = request.Name;
            }

            // 检查编码唯一性（如果要更新编码）
            if (!string.IsNullOrEmpty(request.Code) && request.Code != role.Code)
            {
                var existingRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Code == request.Code && r.Id != request.Id, cancellationToken);
                if (existingRole != null)
                {
                    return Result<bool>.FailResult("角色编码已存在");
                }
                role.Code = request.Code;
            }

            // 更新其他属性
            if (request.Description != null)
            {
                role.Description = request.Description;
            }

            if (request.IsEnabled.HasValue)
            {
                role.IsEnabled = request.IsEnabled.Value;
            }

            role.UpdateTime = DateTime.UtcNow;

            var result = await _roleManager.UpdateAsync(role);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<bool>.FailResult($"更新角色失败: {errors}");
            }

            // 如果提供了权限ID列表，则更新角色权限
            if (request.PermissionIds != null)
                {
                Console.WriteLine($"准备更新角色权限，权限数量: {request.PermissionIds.Count}");
                var permissionResult = await UpdateRolePermissions(request.Id, request.PermissionIds, cancellationToken);
                if (!permissionResult.Success)
                {
                    return Result<bool>.FailResult($"更新角色权限失败: {permissionResult.Message}");
                }
            }
            else
            {
                Console.WriteLine("未提供权限ID列表，仅更新角色基本信息");
            }

            return Result<bool>.SuccessResult(true, "角色及权限更新成功");
        }
        catch (Exception ex)
        {
            return Result<bool>.FailResult($"更新角色时发生错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新角色权限
    /// </summary>
    private async Task<Result<bool>> UpdateRolePermissions(Guid roleId, List<Guid> permissionIds, CancellationToken cancellationToken)
    {
        try
        {
            var role = await _context.Roles
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

            if (role == null)
            {
                return Result<bool>.FailResult("角色不存在");
            }

            // 获取要分配的权限
            var permissions = await _context.Permissions
                .Where(p => permissionIds.Contains(p.Id))
                .ToListAsync(cancellationToken);

            // 验证所有权限都存在
            if (permissions.Count != permissionIds.Count)
            {
                return Result<bool>.FailResult("部分权限不存在");
            }

            // 清空现有权限并添加新权限
            role.Permissions.Clear();
            foreach (var permission in permissions)
            {
                role.Permissions.Add(permission);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.SuccessResult(true, "角色权限更新成功");
        }
        catch (Exception ex)
        {
            return Result<bool>.FailResult($"更新角色权限时发生错误: {ex.Message}");
        }
    }
}

/// <summary>
/// 删除角色命令处理器
/// </summary>
public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, Result<bool>>
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IdentityServiceDbContext _context;

    public DeleteRoleCommandHandler(
        RoleManager<ApplicationRole> roleManager,
        IdentityServiceDbContext context)
    {
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<Result<bool>> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var role = await _roleManager.FindByIdAsync(request.Id.ToString());
            if (role == null)
            {
                return Result<bool>.FailResult("角色不存在");
            }

            // 检查是否有关联的用户（通过查询AspNetUserRoles表）
            var userCount = await _context.Database
                .SqlQueryRaw<int>($"SELECT COUNT(*) FROM AspNetUserRoles WHERE RoleId = '{request.Id}'")
                .FirstOrDefaultAsync(cancellationToken);

            if (userCount > 0)
            {
                return Result<bool>.FailResult("该角色下有关联用户，无法删除");
            }

            var result = await _roleManager.DeleteAsync(role);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result<bool>.FailResult($"删除角色失败: {errors}");
            }

            return Result<bool>.SuccessResult(true, "角色删除成功");
        }
        catch (Exception ex)
        {
            return Result<bool>.FailResult($"删除角色时发生错误: {ex.Message}");
        }
    }
}