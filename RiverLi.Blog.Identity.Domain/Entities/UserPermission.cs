using RiverLi.DDD.Core.Domain.Common;

namespace RiverLi.Blog.Identity.Domain.Entities;

/// <summary>
/// 用户 - 权限关联表（多对多关系）
/// </summary>
public class UserPermission : IAggregateRoot
{
    public Guid UserId { get; set; }
    public Guid PermissionId { get; set; }
    
    // 导航属性
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
    
    // 审计字段
    public DateTime? CreateTime { get; set; } = DateTime.UtcNow;
}
