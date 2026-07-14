namespace riverli.blog.services.identity.Domain.Entities;

public class RoleMenu
{
    public Guid RoleId { get; set; }
    public virtual AppRole Role { get; set; } = null!;

    public Guid MenuId { get; set; }
    public virtual Menu Menu { get; set; } = null!;
}