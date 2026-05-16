using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Roles.Queries;

public class AssignPermissionsToRoleCommandHandler : IRequestHandler<AssignPermissionsToRoleCommand, Result<bool>>
{
    private readonly IdentityServiceDbContext _context; 
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<AssignPermissionsToRoleCommandHandler> _logger;

    public AssignPermissionsToRoleCommandHandler(
        IdentityServiceDbContext context, 
        RoleManager<ApplicationRole> roleManager,
        ILogger<AssignPermissionsToRoleCommandHandler> logger)
    {
        _context = context;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(AssignPermissionsToRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("开始分配权限到角色，角色 ID: {RoleId}, 权限数量：{Count}", 
                request.RoleId, request.PermissionIds.Count);
            
            var role = await _context.Roles
                .Include(r => r.Permissions) // 确保加载 Permissions 导航属性
                .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

            if (role == null)
            {
                _logger.LogWarning("角色不存在，ID: {RoleId}", request.RoleId);
                return Result<bool>.FailResult("角色不存在");
            }

            // 获取要分配的权限实体
            var permissions = await _context.Permissions
                .Where(p => request.PermissionIds.Contains(p.Id))
                .ToListAsync(cancellationToken);

            // 清空现有权限
            role.Permissions.Clear();
            // 添加新权限
            permissions.ForEach(p => role.Permissions.Add(p));

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("成功分配权限到角色，角色 ID: {RoleId}", request.RoleId);
            return Result<bool>.SuccessResult(true, "权限分配成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "分配权限到角色失败，角色 ID: {RoleId}", request.RoleId);
            return Result<bool>.FailResult("权限分配失败");
        }
    }
}