using Microsoft.AspNetCore.Identity;

namespace riverli.blog.services.identity.Domain.Entities;

public class AppRole : IdentityRole<Guid>
{
    public string Description { get; set; } = string.Empty;
    public virtual ICollection<RoleMenu> RoleMenus { get; set; } = new List<RoleMenu>();
    public virtual ICollection<RoleApi> RoleApis { get; set; } = new List<RoleApi>();
}