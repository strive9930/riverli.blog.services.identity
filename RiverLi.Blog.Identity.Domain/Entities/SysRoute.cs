using Microsoft.AspNetCore.Identity;
using RiverLi.DDD.Core.Domain.Common;

namespace RiverLi.Blog.Identity.Domain.Entities;

public class SysRoute : IAggregateRoot
{
    public Guid Id { get; set; }
    public string Path { get; set; }           // 匹配路径：如 /api/blog/articles/**
    public string Method { get; set; }         // HTTP 方法：GET, POST, DELETE
    public string RequiredPermission { get; set; } // 访问此路由所需的权限 Code (对应 Permission.Code)
    public bool IsEnabled { get; set; } = true;
    public string ServiceName { get; set; }    // 所属微服务名称
    public Guid? RouteGroupId { get; set; }    // 所属路由分组ID
    public Guid? FrontendRouteId { get; set; } // 关联的前端路由ID
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreateTime { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }
    
    // 导航属性
    public virtual RouteGroup? RouteGroup { get; set; }
    
    /// <summary>
    /// 前端路由与后端路由的关联关系 (通过连接表)
    /// </summary>
    public virtual ICollection<FrontendRouteSysRoute> FrontendRouteSysRoutes { get; set; } = new List<FrontendRouteSysRoute>();
    
    /// <summary>
    /// 关联的前端路由 (便捷访问属性)
    /// </summary>
    public IEnumerable<FrontendRoute> FrontendRoutes => FrontendRouteSysRoutes.Select(r => r.FrontendRoute);
}