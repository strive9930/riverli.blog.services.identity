using RiverLi.DDD.Core.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace RiverLi.Blog.Identity.Domain.Entities;

/// <summary>
/// 菜单实体 - 用于管理系统的菜单项
/// </summary>
public class Menu :BaseEntity<Guid>, IAggregateRoot
{
    
    /// <summary>
    /// 菜单名称
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 菜单标题/显示文本
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 菜单路径（路由路径）
    /// </summary>
    [StringLength(200)]
    public string? Path { get; set; }
    
    /// <summary>
    /// 菜单图标
    /// </summary>
    [StringLength(100)]
    public string? Icon { get; set; }
    
    /// <summary>
    /// 排序序号
    /// </summary>
    public int Sort { get; set; } = 0;
    
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// 是否在菜单中显示
    /// </summary>
    public bool IsVisible { get; set; } = true;
    
    /// <summary>
    /// 父级菜单ID
    /// </summary>
    public Guid? ParentId { get; set; }
    
    /// <summary>
    /// 菜单类型
    /// </summary>
    [Required]
    public MenuType MenuType { get; set; }
    
    /// <summary>
    /// 目标受众（后台管理/用户端）
    /// </summary>
    [Required]
    public MenuTarget TargetAudience { get; set; } = MenuTarget.Admin;
    
    /// <summary>
    /// 关联的前端路由ID（可选）
    /// </summary>
    public Guid? FrontendRouteId { get; set; }
    
    /// <summary>
    /// 所属菜单组ID
    /// </summary>
    public Guid? MenuGroupId { get; set; }
    
    /// <summary>
    /// 访问此菜单所需的权限代码
    /// </summary>
    [StringLength(100)]
    public string? RequiredPermission { get; set; }
    
    /// <summary>
    /// 菜单描述
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// 菜单元数据（JSON格式）
    /// </summary>
    public string? Meta { get; set; }
    
    
    #region 导航属性
    
    /// <summary>
    /// 父级菜单
    /// </summary>
    public virtual Menu? Parent { get; set; }
    
    /// <summary>
    /// 子菜单集合
    /// </summary>
    public virtual ICollection<Menu> Children { get; set; } = new List<Menu>();
    
    /// <summary>
    /// 所属菜单组
    /// </summary>
    public virtual MenuGroup? MenuGroup { get; set; }
    
    /// <summary>
    /// 关联的前端路由
    /// </summary>
    public virtual FrontendRoute? FrontendRoute { get; set; }
    
    #endregion
}

/// <summary>
/// 菜单组实体 - 用于对菜单进行逻辑分组
/// </summary>
public class MenuGroup : IAggregateRoot
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// 菜单组名称
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 菜单组编码（唯一标识）
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// 菜单组描述
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// 菜单组图标
    /// </summary>
    [StringLength(100)]
    public string? Icon { get; set; }
    
    /// <summary>
    /// 排序序号
    /// </summary>
    public int Sort { get; set; } = 0;
    
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreateTime { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }
    
    #region 导航属性
    
    /// <summary>
    /// 包含的菜单项集合
    /// </summary>
    public virtual ICollection<Menu> Menus { get; set; } = new List<Menu>();
    
    #endregion
}

/// <summary>
/// 菜单类型枚举
/// </summary>
public enum MenuType
{
    /// <summary>
    /// 普通菜单项
    /// </summary>
    MenuItem = 1,
    
    /// <summary>
    /// 分组标题（不可点击的分组标签）
    /// </summary>
    GroupHeader = 2,
    
    /// <summary>
    /// 分隔符
    /// </summary>
    Divider = 3,
    
    /// <summary>
    /// 外部链接
    /// </summary>
    ExternalLink = 4
}

/// <summary>
/// 菜单目标受众枚举
/// </summary>
public enum MenuTarget
{
    /// <summary>
    /// 后台管理端
    /// </summary>
    Admin = 0,
    
    /// <summary>
    /// 用户端（前台）
    /// </summary>
    User = 1
}