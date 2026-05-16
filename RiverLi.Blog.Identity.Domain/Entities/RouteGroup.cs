using RiverLi.DDD.Core.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace RiverLi.Blog.Identity.Domain.Entities;

/// <summary>
/// 路由分组实体
/// 用于对前端路由和后端路由进行逻辑分组管理
/// </summary>
public class RouteGroup : IAggregateRoot
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// 分组名称
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 分组编码（唯一标识）
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// 分组描述
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// 分组图标
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
    /// 父级分组ID（支持分组嵌套）
    /// </summary>
    public Guid? ParentId { get; set; }
    
    /// <summary>
    /// 分组类型
    /// </summary>
    [Required]
    public RouteGroupType GroupType { get; set; }
    
    /// <summary>
    /// 关联的权限Code（可选）
    /// </summary>
    [StringLength(100)]
    public string? RequiredPermission { get; set; }
    
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
    /// 父级分组
    /// </summary>
    public virtual RouteGroup? Parent { get; set; }
    
    /// <summary>
    /// 子分组集合
    /// </summary>
    public virtual ICollection<RouteGroup> Children { get; set; } = new List<RouteGroup>();
    
    /// <summary>
    /// 关联的前端路由集合
    /// </summary>
    public virtual ICollection<FrontendRoute> FrontendRoutes { get; set; } = new List<FrontendRoute>();
    
    /// <summary>
    /// 关联的后端路由集合
    /// </summary>
    public virtual ICollection<SysRoute> BackendRoutes { get; set; } = new List<SysRoute>();
    
    #endregion
}

/// <summary>
/// 路由分组类型枚举
/// </summary>
public enum RouteGroupType
{
    /// <summary>
    /// 前端路由分组
    /// </summary>
    Frontend = 1,
    
    /// <summary>
    /// 后端路由分组
    /// </summary>
    Backend = 2,
    
    /// <summary>
    /// 混合分组（同时包含前后端路由）
    /// </summary>
    Mixed = 3
}