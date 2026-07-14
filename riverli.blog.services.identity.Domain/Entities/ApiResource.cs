using RiverLi.DDD.Core.Domain.Common;

namespace riverli.blog.services.identity.Domain.Entities;

/// <summary>
/// API 资源实体（记录全系统所有微服务的真实接口）
/// </summary>
public class ApiResource : IAggregateRoot, IEntity<Guid>
{
    /// <summary>归属微服务（如 identity、blog）</summary>
    public string ServiceName { get;  set; } = string.Empty;

    /// <summary>请求方法（GET/POST/PUT/DELETE）</summary>
    public string Method { get;  set; } = string.Empty;

    /// <summary>路由路径（如 /api/identity/users）</summary>
    public string Route { get;  set; } = string.Empty;

    /// <summary>接口描述（如 新增用户）</summary>
    public string Description { get;  set; } = string.Empty;

    /// <summary>是否为公开接口（无需鉴权即可访问）</summary>
    public bool IsPublic { get;  set; }

    /// <summary>关联的角色-API 多对多集合</summary>
    public virtual ICollection<RoleApi> RoleApis { get; set; } = new List<RoleApi>();

    public Guid Id { get; set; }
}
