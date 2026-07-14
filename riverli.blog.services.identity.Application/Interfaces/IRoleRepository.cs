namespace riverli.blog.services.identity.Application.Interfaces;

public interface IRoleRepository
{
    /// <summary>
    /// 获取指定角色的菜单权限 ID 集合
    /// </summary>
    Task<List<Guid>> GetRoleMenuIdsAsync(Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新角色的菜单权限（先删后增）
    /// </summary>
    Task UpdateRoleMenusAsync(Guid roleId, List<Guid> menuIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取指定角色的 API 权限 ID 集合
    /// </summary>
    Task<List<Guid>> GetRoleApiIdsAsync(Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新角色的 API 权限（先删后增）
    /// </summary>
    Task UpdateRoleApisAsync(Guid roleId, List<Guid> apiIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据 API ID 集合删除关联的角色-API 记录（用于同步/上报时清理废弃接口）
    /// </summary>
    Task DeleteRoleApisByApiIdsAsync(List<Guid> apiIds, CancellationToken cancellationToken = default);
}
