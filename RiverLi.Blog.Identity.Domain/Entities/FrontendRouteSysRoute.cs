using RiverLi.DDD.Core.Domain.Common;

namespace RiverLi.Blog.Identity.Domain.Entities;

/// <summary>
/// 前端路由与后端路由的关联实体（多对多关系的连接表）
/// </summary>
public class FrontendRouteSysRoute : IAggregateRoot
{
    public Guid FrontendRouteId { get; set; }
    public Guid SysRouteId { get; set; }
    
    // 审计字段
    public DateTime? CreateTime { get; set; } = DateTime.UtcNow;
    public DateTime? UpdateTime { get; set; }
    
    // 导航属性
    public virtual FrontendRoute FrontendRoute { get; set; } = null!;
    public virtual SysRoute SysRoute { get; set; } = null!;
}