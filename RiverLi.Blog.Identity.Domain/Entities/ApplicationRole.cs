using Microsoft.AspNetCore.Identity;
using RiverLi.DDD.Core.Domain.Common;

namespace RiverLi.Blog.Identity.Domain.Entities
{
    // 继承 IdentityRole<Guid> 并实现 IAggregateRoot
    public class ApplicationRole : IdentityRole<Guid>, IAggregateRoot
    {
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsEnabled { get; set; } = true;
        
        // 父角色ID（用于角色继承）
        public Guid? ParentRoleId { get; set; }
        
        // 审计字段
        public DateTime? CreateTime { get; set; } = DateTime.UtcNow;
        public DateTime? UpdateTime { get; set; }

        // 导航属性
        public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
        public virtual ApplicationRole? ParentRole { get; set; }
        public virtual ICollection<ApplicationRole> ChildRoles { get; set; } = new List<ApplicationRole>();
        public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}