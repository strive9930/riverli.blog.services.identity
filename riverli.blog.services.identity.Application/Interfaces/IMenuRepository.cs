using riverli.blog.services.identity.Application.DTOs;
using riverli.blog.services.identity.Domain.Entities;

namespace riverli.blog.services.identity.Application.Interfaces;

public interface IMenuRepository
{
    /// <summary>
    /// 根据用户ID获取该用户可见的菜单
    /// </summary>
    Task<List<MenuDto>> GetUserMenusAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据用户ID获取所有的权限标识 (如 "sys:user:add")
    /// </summary>
    Task<List<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 创建菜单并保存到数据库
    /// </summary>
    Task CreateMenuAsync(Menu menu, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据ID获取菜单
    /// </summary>
    Task<Menu?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查指定节点下是否存在子菜单
    /// </summary>
    Task<bool> HasChildrenAsync(Guid parentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新菜单并保存变更
    /// </summary>
    Task UpdateAsync(Menu menu, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除菜单（含级联清理角色关联），并保存变更
    /// </summary>
    Task DeleteAsync(Menu menu, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取所有菜单（用于构建菜单树）
    /// </summary>
    Task<List<Menu>> GetAllMenusAsync(CancellationToken cancellationToken = default);
}