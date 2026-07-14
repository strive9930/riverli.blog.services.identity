using Microsoft.EntityFrameworkCore;
using riverli.blog.services.identity.Application.Interfaces;
using riverli.blog.services.identity.Domain.Entities;
using riverli.blog.services.identity.Infrastructure.Data;

namespace riverli.blog.services.identity.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _dbContext;

    public RoleRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Guid>> GetRoleMenuIdsAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RoleMenus
            .Where(rm => rm.RoleId == roleId)
            .Select(rm => rm.MenuId)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateRoleMenusAsync(Guid roleId, List<Guid> menuIds, CancellationToken cancellationToken = default)
    {
        // 1. 移除该角色现有的所有菜单关联
        var existingMenus = await _dbContext.RoleMenus
            .Where(rm => rm.RoleId == roleId)
            .ToListAsync(cancellationToken);
        _dbContext.RoleMenus.RemoveRange(existingMenus);

        // 2. 添加新的菜单关联
        if (menuIds.Count > 0)
        {
            var newRoleMenus = menuIds
                .Select(menuId => new RoleMenu { RoleId = roleId, MenuId = menuId })
                .ToList();
            await _dbContext.RoleMenus.AddRangeAsync(newRoleMenus, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Guid>> GetRoleApiIdsAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.RoleApis
            .Where(ra => ra.RoleId == roleId)
            .Select(ra => ra.ApiId)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateRoleApisAsync(Guid roleId, List<Guid> apiIds, CancellationToken cancellationToken = default)
    {
        // 1. 移除该角色现有的所有 API 关联
        var existingApis = await _dbContext.RoleApis
            .Where(ra => ra.RoleId == roleId)
            .ToListAsync(cancellationToken);
        _dbContext.RoleApis.RemoveRange(existingApis);

        // 2. 添加新的 API 关联
        if (apiIds.Count > 0)
        {
            var newRoleApis = apiIds
                .Select(apiId => new RoleApi { RoleId = roleId, ApiId = apiId })
                .ToList();
            await _dbContext.RoleApis.AddRangeAsync(newRoleApis, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteRoleApisByApiIdsAsync(List<Guid> apiIds, CancellationToken cancellationToken = default)
    {
        var roleApis = await _dbContext.RoleApis
            .Where(ra => apiIds.Contains(ra.ApiId))
            .ToListAsync(cancellationToken);
        if (roleApis.Any())
        {
            _dbContext.RoleApis.RemoveRange(roleApis);
        }
    }
}
