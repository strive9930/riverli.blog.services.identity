using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;

namespace RiverLi.Blog.Identity.Api
{
    public static class PermissionSeedData
    {
        public static async Task EnsurePermissionSeedData(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IdentityServiceDbContext>();

            try
            {
                Console.WriteLine("开始初始化权限数据...");

                // 创建权限项
                var permissions = CreatePermissions();
                
                // 总是尝试添加权限（如果不存在）
                foreach (var permission in permissions)
                {
                    if (!await context.Permissions.AnyAsync(p => p.Code == permission.Code))
                    {
                        context.Permissions.Add(permission);
                    }
                }
                await context.SaveChangesAsync();

                // 总是为角色分配权限
                await AssignPermissionsToAdminRole(context, permissions);
                await AssignPermissionsToUserRole(context, permissions);

                Console.WriteLine("权限数据初始化完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"权限数据初始化失败: {ex.Message}");
                throw;
            }
        }

        private static List<Permission> CreatePermissions()
        {
            return new List<Permission>
            {
                // 仪表盘权限
                new Permission
                {
                    Id = Guid.NewGuid(),
                    Code = "dashboard.view",
                    Name = "查看仪表盘",
                    Description = "允许查看系统仪表盘",
                    Group = "Dashboard",
                    ClaimType = "Permission",
                    ClaimValue = "dashboard.view",
                    RoleId = ""
                },

                // 用户管理权限
                new Permission
                {
                    Id = Guid.NewGuid(),
                    Code = "users.view",
                    Name = "查看用户",
                    Description = "允许查看用户列表",
                    Group = "User Management",
                    ClaimType = "Permission",
                    ClaimValue = "users.view",
                    RoleId = ""
                },
                new Permission
                {
                    Id = Guid.NewGuid(),
                    Code = "users.create",
                    Name = "创建用户",
                    Description = "允许创建新用户",
                    Group = "User Management",
                    ClaimType = "Permission",
                    ClaimValue = "users.create",
                    RoleId = ""
                },
                new Permission
                {
                    Id = Guid.NewGuid(),
                    Code = "users.edit",
                    Name = "编辑用户",
                    Description = "允许编辑用户信息",
                    Group = "User Management",
                    ClaimType = "Permission",
                    ClaimValue = "users.edit",
                    RoleId = ""
                },
                new Permission
                {
                    Id = Guid.NewGuid(),
                    Code = "users.delete",
                    Name = "删除用户",
                    Description = "允许删除用户",
                    Group = "User Management",
                    ClaimType = "Permission",
                    ClaimValue = "users.delete",
                    RoleId = ""
                },

                // 角色管理权限
                new Permission
                {
                    Id = Guid.NewGuid(),
                    Code = "roles.view",
                    Name = "查看角色",
                    Description = "允许查看角色列表",
                    Group = "Role Management",
                    ClaimType = "Permission",
                    ClaimValue = "roles.view",
                    RoleId = ""
                },
                new Permission
                {
                    Id = Guid.NewGuid(),
                    Code = "roles.create",
                    Name = "创建角色",
                    Description = "允许创建新角色",
                    Group = "Role Management",
                    ClaimType = "Permission",
                    ClaimValue = "roles.create",
                    RoleId = ""
                },
                new Permission
                {
                    Id = Guid.NewGuid(),
                    Code = "roles.edit",
                    Name = "编辑角色",
                    Description = "允许编辑角色信息",
                    Group = "Role Management",
                    ClaimType = "Permission",
                    ClaimValue = "roles.edit",
                    RoleId = ""
                },
                new Permission
                {
                    Id = Guid.NewGuid(),
                    Code = "roles.delete",
                    Name = "删除角色",
                    Description = "允许删除角色",
                    Group = "Role Management",
                    ClaimType = "Permission",
                    ClaimValue = "roles.delete",
                    RoleId = ""
                },

                // 权限管理权限
                new Permission
                {
                    Id = Guid.NewGuid(),
                    Code = "permissions.view",
                    Name = "查看权限",
                    Description = "允许查看权限列表",
                    Group = "Permission Management",
                    ClaimType = "Permission",
                    ClaimValue = "permissions.view",
                    RoleId = ""
                },
                new Permission
                {
                    Id = Guid.NewGuid(),
                    Code = "permissions.create",
                    Name = "创建权限",
                    Description = "允许创建新权限",
                    Group = "Permission Management",
                    ClaimType = "Permission",
                    ClaimValue = "permissions.create",
                    RoleId = ""
                },
                new Permission
                {
                    Id = Guid.NewGuid(),
                    Code = "permissions.edit",
                    Name = "编辑权限",
                    Description = "允许编辑权限信息",
                    Group = "Permission Management",
                    ClaimType = "Permission",
                    ClaimValue = "permissions.edit",
                    RoleId = ""
                },
                new Permission
                {
                    Id = Guid.NewGuid(),
                    Code = "permissions.delete",
                    Name = "删除权限",
                    Description = "允许删除权限",
                    Group = "Permission Management",
                    ClaimType = "Permission",
                    ClaimValue = "permissions.delete",
                    RoleId = ""
                },

                // 菜单管理权限
                new Permission
                {
                    Id = Guid.NewGuid(),
                    Code = "menus.view",
                    Name = "查看菜单",
                    Description = "允许查看菜单列表",
                    Group = "Menu Management",
                    ClaimType = "Permission",
                    ClaimValue = "menus.view",
                    RoleId = ""
                },
                new Permission
                {
                    Id = Guid.NewGuid(),
                    Code = "menus.create",
                    Name = "创建菜单",
                    Description = "允许创建新菜单",
                    Group = "Menu Management",
                    ClaimType = "Permission",
                    ClaimValue = "menus.create",
                    RoleId = ""
                },
                new Permission
                {
                    Id = Guid.NewGuid(),
                    Code = "menus.edit",
                    Name = "编辑菜单",
                    Description = "允许编辑菜单信息",
                    Group = "Menu Management",
                    ClaimType = "Permission",
                    ClaimValue = "menus.edit",
                    RoleId = ""
                },
                new Permission
                {
                    Id = Guid.NewGuid(),
                    Code = "menus.delete",
                    Name = "删除菜单",
                    Description = "允许删除菜单",
                    Group = "Menu Management",
                    ClaimType = "Permission",
                    ClaimValue = "menus.delete",
                    RoleId = ""
                },

                // 内容管理权限
                new Permission
                {
                    Id = Guid.NewGuid(),
                    Code = "articles.view",
                    Name = "查看文章",
                    Description = "允许查看文章列表",
                    Group = "Content Management",
                    ClaimType = "Permission",
                    ClaimValue = "articles.view",
                    RoleId = ""
                },
                new Permission
                {
                    Id = Guid.NewGuid(),
                    Code = "articles.create",
                    Name = "创建文章",
                    Description = "允许创建新文章",
                    Group = "Content Management",
                    ClaimType = "Permission",
                    ClaimValue = "articles.create",
                    RoleId = ""
                },
                new Permission
                {
                    Id = Guid.NewGuid(),
                    Code = "articles.edit",
                    Name = "编辑文章",
                    Description = "允许编辑文章",
                    Group = "Content Management",
                    ClaimType = "Permission",
                    ClaimValue = "articles.edit",
                    RoleId = ""
                },
                new Permission
                {
                    Id = Guid.NewGuid(),
                    Code = "articles.delete",
                    Name = "删除文章",
                    Description = "允许删除文章",
                    Group = "Content Management",
                    ClaimType = "Permission",
                    ClaimValue = "articles.delete",
                    RoleId = ""
                },
                new Permission
                {
                    Id = Guid.NewGuid(),
                    Code = "categories.view",
                    Name = "查看分类",
                    Description = "允许查看分类列表",
                    Group = "Content Management",
                    ClaimType = "Permission",
                    ClaimValue = "categories.view",
                    RoleId = ""
                },
                new Permission
                {
                    Id = Guid.NewGuid(),
                    Code = "categories.create",
                    Name = "创建分类",
                    Description = "允许创建新分类",
                    Group = "Content Management",
                    ClaimType = "Permission",
                    ClaimValue = "categories.create",
                    RoleId = ""
                },
                new Permission
                {
                    Id = Guid.NewGuid(),
                    Code = "categories.edit",
                    Name = "编辑分类",
                    Description = "允许编辑分类",
                    Group = "Content Management",
                    ClaimType = "Permission",
                    ClaimValue = "categories.edit",
                    RoleId = ""
                },
                new Permission
                {
                    Id = Guid.NewGuid(),
                    Code = "categories.delete",
                    Name = "删除分类",
                    Description = "允许删除分类",
                    Group = "Content Management",
                    ClaimType = "Permission",
                    ClaimValue = "categories.delete",
                    RoleId = ""
                },

                // 数据分析权限
                new Permission
                {
                    Id = Guid.NewGuid(),
                    Code = "analytics.overview.view",
                    Name = "查看数据概览",
                    Description = "允许查看数据概览",
                    Group = "Analytics",
                    ClaimType = "Permission",
                    ClaimValue = "analytics.overview.view",
                    RoleId = ""
                },
                new Permission
                {
                    Id = Guid.NewGuid(),
                    Code = "analytics.users.view",
                    Name = "查看用户分析",
                    Description = "允许查看用户分析数据",
                    Group = "Analytics",
                    ClaimType = "Permission",
                    ClaimValue = "analytics.users.view",
                    RoleId = ""
                }
            };
        }

        private static async Task AssignPermissionsToAdminRole(IdentityServiceDbContext context, List<Permission> permissions)
        {
            var adminRole = await context.Set<ApplicationRole>()
                .FirstOrDefaultAsync(r => r.Name == "Admin");

            if (adminRole != null)
            {
                // 为管理员角色分配所有权限
                foreach (var permission in permissions)
                {
                    var rolePermission = new
                    {
                        RoleId = adminRole.Id,
                        PermissionId = permission.Id
                    };
                    
                    await context.Database.ExecuteSqlRawAsync(
                        "INSERT IGNORE INTO RolePermissions (RoleId, PermissionId) VALUES ({0}, {1})",
                        rolePermission.RoleId, rolePermission.PermissionId);
                }

                Console.WriteLine($"为管理员角色分配了 {permissions.Count} 个权限");
            }
        }

        private static async Task AssignPermissionsToUserRole(IdentityServiceDbContext context, List<Permission> permissions)
        {
            var userRole = await context.Set<ApplicationRole>()
                .FirstOrDefaultAsync(r => r.Name == "User");

            if (userRole != null)
            {
                // 为普通用户角色分配基本的查看权限：.view 类权限。对安全管理操作不赋予。
                var excludeHighPriv = new[]
                {
                    "users.view", "roles.view", "permissions.view", "menus.view"
                };

                var basicViewPermissions = permissions.Where(p => 
                    p.Code.EndsWith(".view") && 
                    !excludeHighPriv.Contains(p.Code)
                ).ToList();

                foreach (var permission in basicViewPermissions)
                {
                    var rolePermission = new
                    {
                        RoleId = userRole.Id,
                        PermissionId = permission.Id
                    };
                    
                    await context.Database.ExecuteSqlRawAsync(
                        "INSERT IGNORE INTO RolePermissions (RoleId, PermissionId) VALUES ({0}, {1})",
                        rolePermission.RoleId, rolePermission.PermissionId);
                }

                Console.WriteLine($"为普通用户角色分配了 {basicViewPermissions.Count} 个基本查看权限");
            }
            else
            {
                Console.WriteLine("警告：未找到 User 角色，无法为普通用户分配基础权限。");
            }
        }
    }
}