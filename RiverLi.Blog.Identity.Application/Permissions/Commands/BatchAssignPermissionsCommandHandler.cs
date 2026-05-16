using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RiverLi.Blog.Identity.Application.Permissions.Commands;
using RiverLi.Blog.Identity.Application.Permissions.Queries;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Permissions.Commands;

/// <summary>
/// 批量分配权限命令处理器
/// </summary>
public class BatchAssignPermissionsCommandHandler : IRequestHandler<BatchAssignPermissionsCommand, Result<bool>>
{
    private readonly IdentityServiceDbContext _context;
    private readonly ILogger<BatchAssignPermissionsCommandHandler> _logger;

    public BatchAssignPermissionsCommandHandler(
        IdentityServiceDbContext context,
        ILogger<BatchAssignPermissionsCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(BatchAssignPermissionsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("开始批量分配权限，角色 ID: {RoleId}, 添加：{AddCount}, 移除：{RemoveCount}", 
                request.RoleId, request.AddPermissionIds.Count, request.RemovePermissionIds.Count);
                
            // 验证角色是否存在
            var role = await _context.Roles
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);
    
            if (role == null)
            {
                _logger.LogWarning("角色不存在，ID: {RoleId}", request.RoleId);
                return Result<bool>.FailResult("角色不存在");
            }
    
            // 获取所有相关权限
            var allPermissionIds = request.AddPermissionIds.Concat(request.RemovePermissionIds).Distinct().ToList();
            var permissions = await _context.Permissions
                .Where(p => allPermissionIds.Contains(p.Id))
                .ToListAsync(cancellationToken);
    
            // 验证权限是否存在
            var missingPermissions = allPermissionIds.Except(permissions.Select(p => p.Id)).ToList();
            if (missingPermissions.Any())
            {
                _logger.LogWarning("以下权限不存在：{MissingPermissions}", string.Join(", ", missingPermissions));
                return Result<bool>.FailResult($"以下权限不存在：{string.Join(", ", missingPermissions)}");
            }
    
            // 移除权限
            if (request.RemovePermissionIds.Any())
            {
                var permissionsToRemove = permissions
                    .Where(p => request.RemovePermissionIds.Contains(p.Id))
                    .ToList();
                    
                foreach (var permission in permissionsToRemove)
                {
                    role.Permissions.Remove(permission);
                }
            }
    
            // 添加权限
            if (request.AddPermissionIds.Any())
            {
                var permissionsToAdd = permissions
                    .Where(p => request.AddPermissionIds.Contains(p.Id))
                    .Except(role.Permissions) // 避免重复添加
                    .ToList();
                    
                foreach (var permission in permissionsToAdd)
                {
                    role.Permissions.Add(permission);
                }
            }
    
            await _context.SaveChangesAsync(cancellationToken);
    
            _logger.LogInformation("成功更新角色权限，角色 ID: {RoleId}", request.RoleId);
            return Result<bool>.SuccessResult(true, $"成功更新角色权限：添加{request.AddPermissionIds.Count}个，移除{request.RemovePermissionIds.Count}个");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量分配权限失败，角色 ID: {RoleId}", request.RoleId);
            return Result<bool>.FailResult("批量分配权限失败");
        }
    }
}