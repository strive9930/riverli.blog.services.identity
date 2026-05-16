using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RiverLi.Blog.Identity.Domain.Entities;

namespace RiverLi.Blog.Identity.Infrastructure.Data.Configurations;

public class AuthConfiguration : 
    IEntityTypeConfiguration<ApplicationRole>,
    IEntityTypeConfiguration<Permission>,
    IEntityTypeConfiguration<SysRoute>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        // 配置角色与权限的多对多中间表
        builder.HasMany(r => r.Permissions)
            .WithMany(p => p.Roles)
            .UsingEntity<Dictionary<string, object>>(
                "RolePermissions",
                j => j.HasOne<Permission>().WithMany().HasForeignKey("PermissionId"),
                j => j.HasOne<ApplicationRole>().WithMany().HasForeignKey("RoleId")
            );
    }

    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.Code).IsUnique(); // 权限 Code 唯一
        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
    }

    public void Configure(EntityTypeBuilder<SysRoute> builder)
    {
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => new { s.Path, s.Method }).IsUnique();
    }
}