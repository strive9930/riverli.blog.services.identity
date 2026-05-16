using RiverLi.DDD.Core.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace RiverLi.Blog.Identity.Domain.Entities;

/// <summary>
/// 前端路由实体 - 用于管理前端页面路由和菜单
/// </summary>
public class FrontendRoute : IAggregateRoot
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// 路由路径 (如: /dashboard/users)
    /// </summary>
    public string Path { get; set; } = string.Empty;
    
    /// <summary>
    /// 路由名称 (用于路由跳转和标识)
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 组件路径 (前端组件文件路径)
    /// </summary>
    public string Component { get; set; } = string.Empty;
    
    /// <summary>
    /// 路由标题/显示名称
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 图标名称 (如: UserOutlined, HomeFilled)
    /// </summary>
    public string? Icon { get; set; }
    
    /// <summary>
    /// 排序序号 (数值越小越靠前)
    /// </summary>
    public int Sort { get; set; } = 0;
    
    /// <summary>
    /// 父级路由ID (用于构建层级菜单)
    /// </summary>
    public Guid? ParentId { get; set; }
    
    /// <summary>
    /// 是否在菜单中显示
    /// </summary>
    public bool IsMenu { get; set; } = true;
    
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// 所需权限码 (用于权限控制)
    /// </summary>
    public string? RequiredPermission { get; set; }
    
    /// <summary>
    /// 路由描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 路由元数据 (JSON格式，存储额外配置)
    /// </summary>
    public string? Meta { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreateTime { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }
    
    /// <summary>
    /// 所属路由分组ID
    /// </summary>
    public Guid? RouteGroupId { get; set; }
    
    #region 导航属性
    
    /// <summary>
    /// 父级路由
    /// </summary>
    public virtual FrontendRoute? Parent { get; set; }
    
    /// <summary>
    /// 子路由集合
    /// </summary>
    public virtual ICollection<FrontendRoute> Children { get; set; } = new List<FrontendRoute>();
    
    /// <summary>
    /// 前端路由与后端路由的关联关系 (通过连接表)
    /// </summary>
    public virtual ICollection<FrontendRouteSysRoute> FrontendRouteSysRoutes { get; set; } = new List<FrontendRouteSysRoute>();
    
    /// <summary>
    /// 关联的后端路由 (便捷访问属性)
    /// </summary>
    public ICollection<SysRoute> BackendRoutes { get; set; } = new List<SysRoute>();
    
    /// <summary>
    /// 所属路由分组
    /// </summary>
    public virtual RouteGroup? RouteGroup { get; set; }
    
    #endregion
    
    /// <summary>
    /// 创建前端路由的工厂方法
    /// </summary>
    public static FrontendRoute Create(
        string path,
        string name,
        string component,
        string title,
        int sort = 0,
        Guid? parentId = null,
        string? icon = null,
        bool isMenu = true,
        string? requiredPermission = null,
        Guid? routeGroupId = null)
    {
        return new FrontendRoute
        {
            Id = Guid.NewGuid(),
            Path = path,
            Name = name,
            Component = component,
            Title = title,
            Icon = icon,
            Sort = sort,
            ParentId = parentId,
            IsMenu = isMenu,
            RequiredPermission = requiredPermission,
            RouteGroupId = routeGroupId,
            CreateTime = DateTime.UtcNow
        };
    }
}