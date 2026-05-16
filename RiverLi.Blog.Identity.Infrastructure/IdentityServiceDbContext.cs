using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Domain.Entities;

namespace RiverLi.Blog.Identity.Infrastructure.Data
{
    public class IdentityServiceDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public IdentityServiceDbContext(DbContextOptions<IdentityServiceDbContext> options) : base(options)
        {
        }

        // DbSet 属性
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<SysRoute> SysRoutes => Set<SysRoute>();
        public DbSet<FrontendRoute> FrontendRoutes => Set<FrontendRoute>();
        public DbSet<RouteGroup> RouteGroups => Set<RouteGroup>();
        public DbSet<Menu> Menus => Set<Menu>();
        public DbSet<MenuGroup> MenuGroups => Set<MenuGroup>();
        public DbSet<UserLoginHistory> UserLoginHistories => Set<UserLoginHistory>();
        // 暂时注释掉连接表，避免编译错误
        public DbSet<FrontendRouteSysRoute> FrontendRouteSysRoutes => Set<FrontendRouteSysRoute>();
        
        // 用户 - 权限关联表
        public DbSet<Domain.Entities.UserPermission> UserPermissions => Set<Domain.Entities.UserPermission>();
        
        

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 配置 Identity 表名
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("Users");
            });

            builder.Entity<ApplicationRole>(entity =>
            {
                entity.ToTable("Roles");
                entity.Property(r => r.Code).HasMaxLength(50);
                entity.Property(r => r.Description).HasMaxLength(500);
                entity.Property(r => r.IsEnabled).HasDefaultValue(true);
                entity.HasIndex(r => r.Code).IsUnique();
            });

            // IdentityUserRole<Guid> 的表映射由 Identity 框架自动处理
            // 不需要手动配置，避免表映射冲突

            builder.Entity<IdentityUserClaim<Guid>>(entity =>
            {
                entity.ToTable("UserClaims");
            });

            builder.Entity<IdentityUserLogin<Guid>>(entity =>
            {
                entity.ToTable("UserLogins");
            });

            builder.Entity<IdentityRoleClaim<Guid>>(entity =>
            {
                entity.ToTable("RoleClaims");
            });

            builder.Entity<IdentityUserToken<Guid>>(entity =>
            {
                entity.ToTable("UserTokens");
            });

            // 配置角色与权限的多对多关系
            builder.Entity<ApplicationRole>(entity =>
            {
                entity.HasMany(r => r.Permissions)
                    .WithMany(p => p.Roles)
                    .UsingEntity<Dictionary<string, object>>(
                        "RolePermissions",
                        j => j.HasOne<Permission>().WithMany().HasForeignKey("PermissionId"),
                        j => j.HasOne<ApplicationRole>().WithMany().HasForeignKey("RoleId"),
                        j => j.ToTable("RolePermissions")
                    );
                
                // 配置角色继承关系
                entity.HasOne(r => r.ParentRole)
                    .WithMany(r => r.ChildRoles)
                    .HasForeignKey(r => r.ParentRoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 配置用户与权限的多对多关系
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.HasMany(u => u.Permissions)
                    .WithMany(p => p.Users)
                    .UsingEntity<UserPermission>(
                        j => j.HasOne(up => up.Permission).WithMany().HasForeignKey(up => up.PermissionId),
                        j => j.HasOne(up => up.User).WithMany().HasForeignKey(up => up.UserId),
                        j =>
                        {
                            j.HasKey(up => new { up.UserId, up.PermissionId });
                            j.ToTable("UserPermissions");
                        }
                    );
                            
                // 用户与角色的关系已经由 Identity 框架自动处理，无需额外配置
            });

            // 配置 Permission 映射
            builder.Entity<Permission>(entity =>
            {
                entity.ToTable("Permissions");   
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Code).IsRequired().HasMaxLength(50);
                entity.HasIndex(p => p.Code).IsUnique();
            });

            // 配置 SysRoute 映射
            builder.Entity<SysRoute>(entity =>
            {
                entity.ToTable("SysRoutes");
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Path).IsRequired().HasMaxLength(200);
                entity.HasIndex(s => new { s.Path, s.Method }).IsUnique();
                
                entity.HasOne(s => s.RouteGroup)
                    .WithMany(rg => rg.BackendRoutes)
                    .HasForeignKey(s => s.RouteGroupId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // 配置 FrontendRoute 映射
            builder.Entity<FrontendRoute>(entity =>
            {
                entity.ToTable("FrontendRoutes");
                entity.HasKey(f => f.Id);
                entity.Property(f => f.Path).IsRequired().HasMaxLength(200);
                entity.Property(f => f.Name).IsRequired().HasMaxLength(100);
                entity.Property(f => f.Component).IsRequired().HasMaxLength(200);
                entity.Property(f => f.Title).IsRequired().HasMaxLength(100);
                
                entity.HasIndex(f => f.Path).IsUnique();
                entity.HasIndex(f => f.Name).IsUnique();
                entity.HasIndex(f => f.ParentId);
                entity.HasIndex(f => f.RouteGroupId);
                
                entity.HasOne(f => f.Parent)
                    .WithMany(f => f.Children)
                    .HasForeignKey(f => f.ParentId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(f => f.RouteGroup)
                    .WithMany(rg => rg.FrontendRoutes)
                    .HasForeignKey(f => f.RouteGroupId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // 配置 RouteGroup 映射
            builder.Entity<RouteGroup>(entity =>
            {
                entity.ToTable("RouteGroups");
                entity.HasKey(rg => rg.Id);
                entity.Property(rg => rg.Name).IsRequired().HasMaxLength(100);
                entity.Property(rg => rg.Code).IsRequired().HasMaxLength(50);
                entity.Property(rg => rg.Description).HasMaxLength(500);
                entity.Property(rg => rg.Icon).HasMaxLength(100);
                entity.Property(rg => rg.RequiredPermission).HasMaxLength(100);
                
                entity.HasIndex(rg => rg.Code).IsUnique();
                entity.HasIndex(rg => rg.ParentId);
                
                entity.HasOne(rg => rg.Parent)
                    .WithMany(rg => rg.Children)
                    .HasForeignKey(rg => rg.ParentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 配置 Menu 映射
            builder.Entity<Menu>(entity =>
            {
                entity.ToTable("Menus");
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Name).IsRequired().HasMaxLength(100);
                entity.Property(m => m.Title).IsRequired().HasMaxLength(100);
                entity.Property(m => m.Path).HasMaxLength(200);
                entity.Property(m => m.Icon).HasMaxLength(100);
                entity.Property(m => m.Description).HasMaxLength(500);
                
                entity.HasIndex(m => m.Name).IsUnique();
                entity.HasIndex(m => m.ParentId);
                entity.HasIndex(m => m.MenuGroupId);
                entity.HasIndex(m => m.FrontendRouteId);
                
                entity.HasOne(m => m.Parent)
                    .WithMany(m => m.Children)
                    .HasForeignKey(m => m.ParentId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(m => m.MenuGroup)
                    .WithMany(mg => mg.Menus)
                    .HasForeignKey(m => m.MenuGroupId)
                    .OnDelete(DeleteBehavior.SetNull);
                
                entity.HasOne(m => m.FrontendRoute)
                    .WithMany()
                    .HasForeignKey(m => m.FrontendRouteId)
                    .OnDelete(DeleteBehavior.SetNull);
                
                // 配置 TargetAudience 字段
                entity.Property(m => m.TargetAudience)
                    .HasConversion<int>()
                    .HasColumnName("TargetAudience");
            });

            // 配置 MenuGroup 映射
            builder.Entity<MenuGroup>(entity =>
            {
                entity.ToTable("MenuGroups");
                entity.HasKey(mg => mg.Id);
                entity.Property(mg => mg.Name).IsRequired().HasMaxLength(100);
                entity.Property(mg => mg.Code).IsRequired().HasMaxLength(50);
                entity.Property(mg => mg.Description).HasMaxLength(500);
                entity.Property(mg => mg.Icon).HasMaxLength(100);
                
                entity.HasIndex(mg => mg.Code).IsUnique();
                entity.HasIndex(mg => mg.Name).IsUnique();
            });

            // 配置 FrontendRouteSysRoute 映射 (多对多关系连接表)
            builder.Entity<FrontendRouteSysRoute>(entity =>
            {
                entity.ToTable("FrontendRouteSysRoutes");
                entity.HasKey(frs => new { frs.FrontendRouteId, frs.SysRouteId });
                
                // 配置审计字段
                entity.Property(frs => frs.CreateTime).HasColumnName("CreateTime");
                
                entity.HasOne(frs => frs.FrontendRoute)
                    .WithMany(fr => fr.FrontendRouteSysRoutes)
                    .HasForeignKey(frs => frs.FrontendRouteId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(frs => frs.SysRoute)
                    .WithMany(sr => sr.FrontendRouteSysRoutes)
                    .HasForeignKey(frs => frs.SysRouteId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            // 配置 UserLoginHistory 映射
            builder.Entity<UserLoginHistory>(entity =>
            {
                entity.ToTable("UserLoginHistories");
                entity.HasKey(ulh => ulh.Id);
                
                entity.Property(ulh => ulh.IpAddress).IsRequired().HasMaxLength(45);
                entity.Property(ulh => ulh.Location).HasMaxLength(100);
                entity.Property(ulh => ulh.DeviceInfo).IsRequired().HasMaxLength(200);
                entity.Property(ulh => ulh.BrowserInfo).HasMaxLength(100);
                entity.Property(ulh => ulh.OperatingSystem).HasMaxLength(100);
                entity.Property(ulh => ulh.Status).IsRequired().HasMaxLength(20);
                entity.Property(ulh => ulh.FailureReason).HasMaxLength(500);
                entity.Property(ulh => ulh.UserAgent).HasMaxLength(500);
                entity.Property(ulh => ulh.SessionId).HasMaxLength(100);
                
                entity.HasIndex(ulh => ulh.UserId);
                entity.HasIndex(ulh => ulh.CreateTime);
                entity.HasIndex(ulh => ulh.IpAddress);
                entity.HasIndex(ulh => ulh.Status);
                
                entity.HasOne(ulh => ulh.User)
                    .WithMany(u => u.LoginHistories)
                    .HasForeignKey(ulh => ulh.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}