using riverli.blog.services.identity.Domain.Enums;
using RiverLi.DDD.Core.Domain.Common;

namespace riverli.blog.services.identity.Domain.Entities;

/// <summary>
/// 系统菜单实体（纯 UI 导航，不承载接口权限）
/// </summary>
public class Menu : IAggregateRoot, IEntity<Guid>
{
    /// <summary>主键</summary>
    public Guid Id { get; set; }
    /// <summary>路由名称（英文字段）</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>菜单显示标题</summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>路由路径</summary>
    public string Path { get; set; } = string.Empty;
    /// <summary>前端组件路径</summary>
    public string Component { get; set; } = string.Empty;
    /// <summary>图标名称</summary>
    public string Icon { get; set; } = string.Empty;
    /// <summary>父级菜单ID，根节点为 null</summary>
    public Guid? ParentId { get; set; }
    /// <summary>同级排序序号</summary>
    public int SortOrder { get; set; }
    /// <summary>菜单类型：目录 / 页面 / 按钮</summary>
    public MenuType Type { get; set; }

    /// <summary>关联的角色-菜单多对多集合（控制菜单可见性）</summary>
    public virtual ICollection<RoleMenu> RoleMenus { get; set; } = new List<RoleMenu>();
}