namespace riverli.blog.services.identity.Domain.Entities;

/// <summary>
/// 角色-API 关联表（取代 Menu.Permission 在接口拦截层的作用）
/// </summary>
public class RoleApi
{
    /// <summary>角色ID</summary>
    public Guid RoleId { get; set; }

    /// <summary>角色导航属性</summary>
    public virtual AppRole Role { get; set; } = null!;

    /// <summary>API资源ID</summary>
    public Guid ApiId { get; set; }

    /// <summary>API资源导航属性</summary>
    public virtual ApiResource ApiResource { get; set; } = null!;
}
