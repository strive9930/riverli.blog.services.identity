using Microsoft.EntityFrameworkCore;
using riverli.blog.services.identity.Application.DTOs;
using riverli.blog.services.identity.Application.Interfaces;
using riverli.blog.services.identity.Domain.Entities;
using riverli.blog.services.identity.Domain.Enums;
using riverli.blog.services.identity.Infrastructure.Data;

namespace riverli.blog.services.identity.Infrastructure.Repositories;

public class MenuRepository : IMenuRepository
{
    private readonly AppDbContext _context;

    public MenuRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<MenuDto>> GetUserMenusAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // 将原先报错的连表查询逻辑搬到这里来
        var menus = await (from ur in _context.UserRoles
            where ur.UserId == userId
            join rm in _context.RoleMenus on ur.RoleId equals rm.RoleId
            join m in _context.Menus on rm.MenuId equals m.Id
            where m.Type != MenuType.Button // 过滤掉按钮
            orderby m.SortOrder
            select new MenuDto(
                m.Id, m.Name, m.Title, m.Path, m.Component, 
                m.Icon, m.ParentId, m.SortOrder, (int)m.Type
            )).Distinct().ToListAsync(cancellationToken);

        return menus;
    }
    /// <summary>
    /// 实现获取权限标识的方法
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<List<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var permissions = await (from ur in _context.UserRoles
                where ur.UserId == userId
                join ra in _context.RoleApis on ur.RoleId equals ra.RoleId
                join a in _context.ApiResources on ra.ApiId equals a.Id
                select a.Method + ":" + a.Route)
            .Distinct()
            .ToListAsync(cancellationToken);

        return permissions;
    }

    /// <summary>
    /// 创建菜单并保存到数据库
    /// </summary>
    public async Task CreateMenuAsync(Menu menu, CancellationToken cancellationToken = default)
    {
        await _context.Menus.AddAsync(menu, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 根据ID获取菜单
    /// </summary>
    public async Task<Menu?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Menus.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <summary>
    /// 检查指定节点下是否存在子菜单
    /// </summary>
    public async Task<bool> HasChildrenAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        return await _context.Menus.AnyAsync(m => m.ParentId == parentId, cancellationToken);
    }

    /// <summary>
    /// 更新菜单并保存变更
    /// </summary>
    public async Task UpdateAsync(Menu menu, CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 删除菜单（含级联清理角色关联），并保存变更
    /// </summary>
    public async Task DeleteAsync(Menu menu, CancellationToken cancellationToken = default)
    {
        // 级联清理角色关联表
        var roleMenus = await _context.RoleMenus.Where(rm => rm.MenuId == menu.Id).ToListAsync(cancellationToken);
        _context.RoleMenus.RemoveRange(roleMenus);

        _context.Menus.Remove(menu);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 获取所有菜单（用于构建菜单树）
    /// </summary>
    public async Task<List<Menu>> GetAllMenusAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Menus.OrderBy(m => m.SortOrder).ToListAsync(cancellationToken);
    }
}