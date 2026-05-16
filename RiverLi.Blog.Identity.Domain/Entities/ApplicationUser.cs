using Microsoft.AspNetCore.Identity;
using RiverLi.DDD.Core.Domain.Common;

namespace RiverLi.Blog.Identity.Domain.Entities
{
    // 继承 IdentityUser<Guid> 以使用 GUID 作为主键
    // 实现 IAggregateRoot 标记它为聚合根
    public class ApplicationUser : IdentityUser<Guid>, IAggregateRoot
    {
        public string? NickName { get; set; }
        public string? AvatarUrl { get; set; }

        // 审计字段 (因为 IdentityUser 没有包含这些，我们可以手动加上或者组合)
        public DateTime? CreateTime { get; set; } = DateTime.UtcNow;
        public DateTime? UpdateTime { get; set; }
        
        // 权限导航属性
        public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
        
        // 角色导航属性
        public virtual ICollection<ApplicationRole> Roles { get; set; } = new List<ApplicationRole>();
        
        // 登录历史导航属性
        public virtual ICollection<UserLoginHistory> LoginHistories { get; set; } = new List<UserLoginHistory>();
        // 是否启用
        public bool IsEnabled { get; set; }
    }
}