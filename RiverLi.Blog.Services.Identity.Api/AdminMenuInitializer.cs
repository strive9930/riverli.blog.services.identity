using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;

namespace RiverLi.Blog.Identity.Api
{
    /// <summary>
    /// 管理员账号菜单数据初始化器
    /// 负责将菜单、权限与管理员账号关联
    /// </summary>
    public static class AdminMenuInitializer
    {
        public static async Task InitializeAdminMenus(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IdentityServiceDbContext>();

            try
            {
                Console.WriteLine("开始关联管理员菜单权限...");

                // 获取管理员角色
                var adminRole = await context.Set<ApplicationRole>()
                    .FirstOrDefaultAsync(r => r.Name == "Admin");

                if (adminRole == null)
                {
                    Console.WriteLine("未找到 Admin 角色，跳过菜单权限关联");
                    return;
                }

                // 获取所有权限并关联到 Admin 角色
                var allPermissions = await context.Permissions.ToListAsync();
                
                // 获取 Admin 角色已分配的权限 IDs
                var assignedPermissionIds = adminRole.Permissions.Select(p => p.Id).ToHashSet();
                
                foreach (var permission in allPermissions)
                {
                    // 如果还没有关联，则进行关联
                    if (!assignedPermissionIds.Contains(permission.Id))
                    {
                        await context.Database.ExecuteSqlRawAsync(
                            "INSERT IGNORE INTO RolePermissions (RoleId, PermissionId) VALUES ({0}, {1})",
                            adminRole.Id, permission.Id);
                    }
                }

                Console.WriteLine($"管理员角色已关联 {allPermissions.Count} 个权限");
                Console.WriteLine("管理员菜单权限关联完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"管理员菜单权限关联失败：{ex.Message}");
                throw;
            }
        }
    }
}