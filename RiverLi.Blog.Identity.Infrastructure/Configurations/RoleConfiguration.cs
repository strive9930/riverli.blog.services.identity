using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RiverLi.Blog.Identity.Domain.Entities;

namespace RiverLi.Blog.Identity.Infrastructure.Configurations;

public class RoleConfiguration
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasMany(x => x.Permissions).WithMany(x => x.Roles)
            .UsingEntity(j => j.ToTable("RolePermissions")); // ✅ 权限-角色中间表
    }
}