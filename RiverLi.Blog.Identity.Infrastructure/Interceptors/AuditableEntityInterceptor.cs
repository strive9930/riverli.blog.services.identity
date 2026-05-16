using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.DDD.Core.Domain.Common;

namespace RiverLi.Blog.Identity.Infrastructure.Interceptors
{
    /// <summary>
    /// 审计字段拦截器 - 自动填充 CreateTime 和 UpdateTime
    /// </summary>
    public class AuditableEntityInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context == null) return base.SavingChangesAsync(eventData, result, cancellationToken);

            foreach (var entry in context.ChangeTracker.Entries())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        SetCreationAuditFields(entry);
                        break;
                    
                    case EntityState.Modified:
                        SetUpdateAuditFields(entry);
                        break;
                }
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            var context = eventData.Context;
            if (context == null) return base.SavingChanges(eventData, result);

            foreach (var entry in context.ChangeTracker.Entries())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        SetCreationAuditFields(entry);
                        break;
                    
                    case EntityState.Modified:
                        SetUpdateAuditFields(entry);
                        break;
                }
            }

            return base.SavingChanges(eventData, result);
        }

        /// <summary>
        /// 设置创建时的审计字段
        /// </summary>
        private static void SetCreationAuditFields(EntityEntry entry)
        {
            var now = DateTime.UtcNow;

            // 处理继承自 BaseEntity 的实体
            if (entry.Entity is BaseEntity<Guid> baseEntity)
            {
                if (baseEntity.CreateTime == null)
                {
                    baseEntity.CreateTime = now;
                }
            }

            // 处理 ApplicationUser
            if (entry.Entity is ApplicationUser user)
            {
                if (user.CreateTime == null)
                {
                    user.CreateTime = now;
                }
            }

            // 处理 ApplicationRole
            if (entry.Entity is ApplicationRole role)
            {
                if (role.CreateTime == null)
                {
                    role.CreateTime = now;
                }
            }

            // 处理 Permission
            if (entry.Entity is Permission permission)
            {
                if (permission.CreateTime == null)
                {
                    permission.CreateTime = now;
                }
            }

            // 处理 SysRoute
            if (entry.Entity is SysRoute sysRoute)
            {
                if (sysRoute.CreateTime == null)
                {
                    sysRoute.CreateTime = now;
                }
            }

            // 处理 FrontendRoute
            if (entry.Entity is FrontendRoute frontendRoute)
            {
                if (frontendRoute.CreateTime == null)
                {
                    frontendRoute.CreateTime = now;
                }
            }

            // 处理 RouteGroup
            if (entry.Entity is RouteGroup routeGroup)
            {
                if (routeGroup.CreateTime == null)
                {
                    routeGroup.CreateTime = now;
                }
            }

            // 处理 Menu
            if (entry.Entity is Menu menu)
            {
                if (menu.CreateTime == null)
                {
                    menu.CreateTime = now;
                }
            }

            // 处理 MenuGroup
            if (entry.Entity is MenuGroup menuGroup)
            {
                if (menuGroup.CreateTime == null)
                {
                    menuGroup.CreateTime = now;
                }
            }

            // 处理 UserLoginHistory
            if (entry.Entity is UserLoginHistory loginHistory)
            {
                if (loginHistory.CreateTime == null)
                {
                    loginHistory.CreateTime = now;
                }
                if (loginHistory.LoginTime == default)
                {
                    loginHistory.LoginTime = now;
                }
            }

            // 处理 FrontendRouteSysRoute
            if (entry.Entity is FrontendRouteSysRoute frontendRouteSysRoute)
            {
                if (frontendRouteSysRoute.CreateTime == null)
                {
                    frontendRouteSysRoute.CreateTime = now;
                }
            }
        }

        /// <summary>
        /// 设置更新时的审计字段
        /// </summary>
        private static void SetUpdateAuditFields(EntityEntry entry)
        {
            var now = DateTime.UtcNow;

            // 处理继承自 BaseEntity 的实体
            if (entry.Entity is BaseEntity<Guid> baseEntity)
            {
                baseEntity.UpdateTime = now;
            }

            // 处理 ApplicationUser
            if (entry.Entity is ApplicationUser user && entry.Reference("CreateTime").IsModified)
            {
                user.UpdateTime = now;
            }

            // 处理 ApplicationRole
            if (entry.Entity is ApplicationRole role && entry.Reference("CreateTime").IsModified)
            {
                role.UpdateTime = now;
            }

            // 处理 Permission
            if (entry.Entity is Permission permission && entry.Reference("CreateTime").IsModified)
            {
                permission.UpdateTime = now;
            }

            // 处理 SysRoute
            if (entry.Entity is SysRoute sysRoute && entry.Reference("CreateTime").IsModified)
            {
                sysRoute.UpdateTime = now;
            }

            // 处理 FrontendRoute
            if (entry.Entity is FrontendRoute frontendRoute && entry.Reference("CreateTime").IsModified)
            {
                frontendRoute.UpdateTime = now;
            }

            // 处理 RouteGroup
            if (entry.Entity is RouteGroup routeGroup && entry.Property("CreateTime").IsModified)
            {
                routeGroup.UpdateTime = now;
            }

            // 处理 Menu
            if (entry.Entity is Menu menu && entry.Property("CreateTime").IsModified)
            {
                menu.UpdateTime = now;
            }

            // 处理 MenuGroup
            if (entry.Entity is MenuGroup menuGroup && entry.Property("CreateTime").IsModified)
            {
                menuGroup.UpdateTime = now;
            }

            // 处理 FrontendRouteSysRoute
            if (entry.Entity is FrontendRouteSysRoute frontendRouteSysRoute && 
                entry.Property("CreateTime").IsModified)
            {
                frontendRouteSysRoute.UpdateTime = now;
            }
        }
    }
}
