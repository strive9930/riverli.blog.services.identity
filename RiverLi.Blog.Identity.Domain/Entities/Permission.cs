using RiverLi.DDD.Core.Domain.Common;

namespace RiverLi.Blog.Identity.Domain.Entities
{
    public class Permission : IAggregateRoot
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Group { get; set; } = string.Empty;
        public string? ClaimType { get; set; }
        public string? ClaimValue { get; set; }
        public string RoleId { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true; // 启用状态
        
        // 审计字段
        public DateTime? CreateTime { get; set; } = DateTime.UtcNow;
        public DateTime? UpdateTime { get; set; }

        // 导航属性
        public virtual ICollection<ApplicationRole> Roles { get; set; } = new List<ApplicationRole>();
        public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}