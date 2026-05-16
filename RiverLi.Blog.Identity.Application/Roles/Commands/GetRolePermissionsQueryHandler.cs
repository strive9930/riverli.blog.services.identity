using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Roles.Commands;

public class GetRolePermissionsQueryHandler : IRequestHandler<GetRolePermissionsQuery, Result<List<Permission>>>
{
    private readonly IdentityServiceDbContext _context;
    private readonly ILogger<GetRolePermissionsQueryHandler> _logger;

    public GetRolePermissionsQueryHandler(
        IdentityServiceDbContext context,
        ILogger<GetRolePermissionsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<List<Permission>>> Handle(GetRolePermissionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("查询角色权限，角色 ID: {RoleId}", request.RoleId);
            
            // 1. 根据 RoleId 查找角色，并 Include 其关联的 Permissions
            var role = await _context.Roles
                .Include(r => r.Permissions) // 假设 ApplicationRole 实体有 Permissions 导航属性
                .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

            if (role == null)
            {
                _logger.LogWarning("角色不存在，ID: {RoleId}", request.RoleId);
                return Result<List<Permission>>.FailResult("角色不存在");
            }

            // 2. 提取权限列表
            var permissions = role.Permissions.ToList(); // 或者 .AsList()，取决于 EF Core 版本

            _logger.LogInformation("成功获取角色权限，角色 ID: {RoleId}, 数量：{Count}", request.RoleId, permissions.Count);
            return Result<List<Permission>>.SuccessResult(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取角色权限失败，角色 ID: {RoleId}", request.RoleId);
            return Result<List<Permission>>.FailResult("获取角色权限失败");
        }
    }
}