// Infrastructure/Data/AppDbContext.cs

using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Infrastructure.Shared.Data;
using riverli.blog.services.identity.Domain.Entities;
using RiverLi.DDD.Core.Application.Common.Interfaces;

namespace riverli.blog.services.identity.Infrastructure.Data;

public class AppDbContext  : RiverDbContext//: IdentityDbContext<AppUser, AppRole, Guid>
{
    public AppDbContext(DbContextOptions options, IMediator mediator, ICurrentUser currentUser) : base(options, mediator, currentUser)
    {
    }

    public DbSet<Menu> Menus { get; set; }
    public DbSet<RoleMenu> RoleMenus { get; set; }
    public DbSet<ApiResource> ApiResources { get; set; }
    public DbSet<RoleApi> RoleApis { get; set; }
    public DbSet<IdentityUserRole<Guid>> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // 必须首先调用

        builder.Entity<AppUser>().ToTable("sys_users");
        builder.Entity<AppRole>().ToTable("sys_roles");
        builder.Entity<Menu>().ToTable("sys_menus");
        builder.Entity<ApiResource>().ToTable("sys_api_resources");

        builder.Entity<IdentityUserRole<Guid>>(entity =>
        {
            entity.ToTable("sys_user_roles");
            entity.HasKey(r => new { r.UserId, r.RoleId });
        });

        builder.Entity<IdentityUserClaim<Guid>>(entity =>
        {
            entity.ToTable("sys_user_claims");
            entity.HasKey(c => c.Id);
        });

        builder.Entity<IdentityUserLogin<Guid>>(entity =>
        {
            entity.ToTable("sys_user_logins");
            entity.HasKey(l => new { l.LoginProvider, l.ProviderKey });
        });

        builder.Entity<IdentityUserToken<Guid>>(entity =>
        {
            entity.ToTable("sys_user_tokens");
            entity.HasKey(t => new { t.UserId, t.LoginProvider, t.Name });
        });

        builder.Entity<IdentityRoleClaim<Guid>>(entity =>
        {
            entity.ToTable("sys_role_claims");
            entity.HasKey(c => c.Id);
        });

        // 配置 RoleApi 多对多联合主键
        builder.Entity<RoleApi>(entity =>
        {
            entity.ToTable("sys_role_apis");
            entity.HasKey(ra => new { ra.RoleId, ra.ApiId });

            entity.HasOne(ra => ra.Role)
                .WithMany(r => r.RoleApis)
                .HasForeignKey(ra => ra.RoleId);

            entity.HasOne(ra => ra.ApiResource)
                .WithMany(a => a.RoleApis)
                .HasForeignKey(ra => ra.ApiId);
        });

        // 配置 RoleMenu 多对多联合主键
        builder.Entity<RoleMenu>(entity =>
        {
            entity.ToTable("sys_role_menus");
            entity.HasKey(rm => new { rm.RoleId, rm.MenuId });

            entity.HasOne(rm => rm.Role)
                .WithMany(r => r.RoleMenus)
                .HasForeignKey(rm => rm.RoleId);

            entity.HasOne(rm => rm.Menu)
                .WithMany(m => m.RoleMenus)
                .HasForeignKey(rm => rm.MenuId);
        });
    }
}