using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;

namespace RiverLi.Blog.Identity.Api
{
    /// <summary>
    /// 菜单种子数据初始化器
    /// 负责创建默认的菜单组和菜单数据
    /// </summary>
    public static class MenuDataSeeder
    {
        public static async Task EnsureMenuData(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IdentityServiceDbContext>();

            try
            {
                Console.WriteLine("开始初始化菜单数据...");

                // 检查是否已有菜单数据
                if (await context.Menus.AnyAsync())
                {
                    Console.WriteLine("菜单数据已存在，跳过初始化");
                    return;
                }

                // 1. 创建菜单组
                var systemGroup = new MenuGroup
                {
                    Id = Guid.NewGuid(),
                    Name = "系统管理",
                    Code = "SYSTEM",
                    Description = "系统相关功能菜单",
                    Icon = "Setting",
                    Sort = 1,
                    IsEnabled = true,
                    CreateTime = DateTime.UtcNow
                };

                var contentGroup = new MenuGroup
                {
                    Id = Guid.NewGuid(),
                    Name = "内容管理",
                    Code = "CONTENT",
                    Description = "内容相关功能菜单",
                    Icon = "Document",
                    Sort = 2,
                    IsEnabled = true,
                    CreateTime = DateTime.UtcNow
                };

                var userGroup = new MenuGroup
                {
                    Id = Guid.NewGuid(),
                    Name = "用户管理",
                    Code = "USER",
                    Description = "用户相关功能菜单",
                    Icon = "User",
                    Sort = 3,
                    IsEnabled = true,
                    CreateTime = DateTime.UtcNow
                };

                context.MenuGroups.AddRange(systemGroup, contentGroup, userGroup);
                await context.SaveChangesAsync();
                Console.WriteLine($"已创建 {context.MenuGroups.Count()} 个菜单组");

                // 2. 创建系统管理菜单
                var systemMenus = new List<Menu>
                {
                    // 仪表盘
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Name = "Dashboard",
                        Title = "仪表盘",
                        Path = "/dashboard",
                        Icon = "Odometer",
                        Sort = 1,
                        IsEnabled = true,
                        IsVisible = true,
                        MenuType = MenuType.MenuItem,
                        TargetAudience = MenuTarget.Admin,
                        MenuGroupId = systemGroup.Id,
                        RequiredPermission = null, // 无需权限
                        Description = "系统仪表盘",
                        CreateTime = DateTime.UtcNow
                    },
                    // 路由管理（父菜单）
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Name = "RouteManagement",
                        Title = "路由管理",
                        Path = "/system/routes",
                        Icon = "Connection",
                        Sort = 2,
                        IsEnabled = true,
                        IsVisible = true,
                        MenuType = MenuType.MenuItem,
                        TargetAudience = MenuTarget.Admin,
                        MenuGroupId = systemGroup.Id,
                        RequiredPermission = "routes.view",
                        Description = "系统路由管理",
                        CreateTime = DateTime.UtcNow
                    },
                    // 菜单管理
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Name = "MenuManagement",
                        Title = "菜单管理",
                        Path = "/system/menus",
                        Icon = "Menu",
                        Sort = 3,
                        IsEnabled = true,
                        IsVisible = true,
                        MenuType = MenuType.MenuItem,
                        TargetAudience = MenuTarget.Admin,
                        MenuGroupId = systemGroup.Id,
                        RequiredPermission = "menus.view",
                        Description = "系统菜单管理",
                        CreateTime = DateTime.UtcNow
                    },
                    // 权限管理
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Name = "PermissionManagement",
                        Title = "权限管理",
                        Path = "/system/permissions",
                        Icon = "Lock",
                        Sort = 4,
                        IsEnabled = true,
                        IsVisible = true,
                        MenuType = MenuType.MenuItem,
                        TargetAudience = MenuTarget.Admin,
                        MenuGroupId = systemGroup.Id,
                        RequiredPermission = "permissions.view",
                        Description = "系统权限管理",
                        CreateTime = DateTime.UtcNow
                    }
                };

                // 3. 创建内容管理菜单
                var contentMenus = new List<Menu>
                {
                    // 文章管理
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Name = "ArticleManagement",
                        Title = "文章管理",
                        Path = "/content/articles",
                        Icon = "Document",
                        Sort = 1,
                        IsEnabled = true,
                        IsVisible = true,
                        MenuType = MenuType.MenuItem,
                        TargetAudience = MenuTarget.Admin,
                        MenuGroupId = contentGroup.Id,
                        RequiredPermission = null, // 对所有用户可见
                        Description = "文章管理",
                        CreateTime = DateTime.UtcNow
                    },
                    // 分类管理
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Name = "CategoryManagement",
                        Title = "分类管理",
                        Path = "/content/categories",
                        Icon = "Collection",
                        Sort = 2,
                        IsEnabled = true,
                        IsVisible = true,
                        MenuType = MenuType.MenuItem,
                        TargetAudience = MenuTarget.Admin,
                        MenuGroupId = contentGroup.Id,
                        RequiredPermission = "categories.view",
                        Description = "文章分类管理",
                        CreateTime = DateTime.UtcNow
                    },
                    // 标签管理
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Name = "TagManagement",
                        Title = "标签管理",
                        Path = "/content/tags",
                        Icon = "PriceTag",
                        Sort = 3,
                        IsEnabled = true,
                        IsVisible = true,
                        MenuType = MenuType.MenuItem,
                        TargetAudience = MenuTarget.Admin,
                        MenuGroupId = contentGroup.Id,
                        RequiredPermission = "tags.view",
                        Description = "文章标签管理",
                        CreateTime = DateTime.UtcNow
                    }
                };

                // 4. 创建用户管理菜单
                var userMenus = new List<Menu>
                {
                    // 用户列表
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Name = "UserList",
                        Title = "用户列表",
                        Path = "/users/list",
                        Icon = "User",
                        Sort = 1,
                        IsEnabled = true,
                        IsVisible = true,
                        MenuType = MenuType.MenuItem,
                        TargetAudience = MenuTarget.Admin,
                        MenuGroupId = userGroup.Id,
                        RequiredPermission = "users.view",
                        Description = "用户列表管理",
                        CreateTime = DateTime.UtcNow
                    },
                    // 角色管理
                    new Menu
                    {
                        Id = Guid.NewGuid(),
                        Name = "RoleManagement",
                        Title = "角色管理",
                        Path = "/users/roles",
                        Icon = "Avatar",
                        Sort = 2,
                        IsEnabled = true,
                        IsVisible = true,
                        MenuType = MenuType.MenuItem,
                        TargetAudience = MenuTarget.Admin,
                        MenuGroupId = userGroup.Id,
                        RequiredPermission = "roles.view",
                        Description = "角色权限管理",
                        CreateTime = DateTime.UtcNow
                    }
                };

                // 添加所有菜单
                var allMenus = new List<Menu>();
                allMenus.AddRange(systemMenus);
                allMenus.AddRange(contentMenus);
                allMenus.AddRange(userMenus);

                context.Menus.AddRange(allMenus);
                await context.SaveChangesAsync();

                Console.WriteLine($"已创建 {allMenus.Count} 个菜单");
                Console.WriteLine($"  - 系统管理: {systemMenus.Count} 个");
                Console.WriteLine($"  - 内容管理: {contentMenus.Count} 个");
                Console.WriteLine($"  - 用户管理: {userMenus.Count} 个");
                Console.WriteLine("菜单数据初始化完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"菜单数据初始化失败: {ex.Message}");
                throw;
            }
        }
    }
}
