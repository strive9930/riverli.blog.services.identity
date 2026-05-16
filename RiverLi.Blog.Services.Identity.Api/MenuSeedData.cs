using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;

namespace RiverLi.Blog.Identity.Api
{
    public static class MenuSeedData
    {
        public static async Task EnsureMenuSeedData(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IdentityServiceDbContext>();

            try
            {
                Console.WriteLine("开始更新菜单权限...");

                // 更新现有菜单的权限设置，使一些菜单对所有用户可见
                var menusToUpdate = new[]
                {
                    //("Dashboard", (string?)null), // 仪表盘对所有用户可见
                    ("ArticleManagement", (string?)null), // 文章管理对所有用户可见
                    //("DataOverview", (string?)null), // 数据概览对所有用户可见
                };

                foreach (var (menuName, permission) in menusToUpdate)
                {
                    var menu = await context.Menus.FirstOrDefaultAsync(m => m.Name == menuName);
                    if (menu != null)
                    {
                        menu.RequiredPermission = permission;
                        Console.WriteLine($"更新菜单 {menuName} 的权限为: {permission ?? "无权限要求"}");
                    }
                    else
                    {
                        Console.WriteLine($"未找到菜单: {menuName}");
                    }
                }

                await context.SaveChangesAsync();

                // 进一步同步菜单 RequiredPermission 与 Permission 数据
                await SyncMenuPermissions(context);

                Console.WriteLine("菜单权限更新完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"菜单权限更新失败: {ex.Message}");
                throw;
            }
        }

        private static async Task SyncMenuPermissions(IdentityServiceDbContext context)
        {
            var adminRole = await context.Set<ApplicationRole>().FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole == null)
            {
                Console.WriteLine("警告：未找到 Admin 角色，菜单权限同步只会更新权限表，不会写入角色-权限关系");
            }

            var menus = await context.Menus.AsNoTracking().ToListAsync();
            foreach (var menu in menus)
            {
                if (string.IsNullOrWhiteSpace(menu.RequiredPermission))
                    continue;

                var perm = await context.Permissions.FirstOrDefaultAsync(p => p.Code == menu.RequiredPermission);
                if (perm == null)
                {
                    perm = new Permission
                    {
                        Id = Guid.NewGuid(),
                        Code = menu.RequiredPermission,
                        Name = menu.Title ?? menu.Name,
                        Description = $"自动同步生成：{menu.Name} 菜单权限",
                        Group = "AutoSync",
                        ClaimType = "Permission",
                        ClaimValue = menu.RequiredPermission,
                        RoleId = ""
                    };
                    context.Permissions.Add(perm);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"已创建权限：{perm.Code}");
                }

                if (adminRole != null)
                {
                    await context.Database.ExecuteSqlRawAsync(
                        "INSERT IGNORE INTO RolePermissions (RoleId, PermissionId) VALUES ({0}, {1})",
                        adminRole.Id, perm.Id);
                }
            }

            Console.WriteLine("菜单权限同步完成（同步到权限表，Admin 角色绑定可选）");
        }
    }
}