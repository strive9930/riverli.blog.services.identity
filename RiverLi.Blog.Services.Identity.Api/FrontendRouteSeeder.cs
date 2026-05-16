using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;

namespace RiverLi.Blog.Identity.Api
{
    /// <summary>
    /// 前端路由种子数据初始化器
    /// 负责初始化路由分组和前端路由数据
    /// </summary>
    public static class FrontendRouteSeeder
    {
        public static async Task EnsureFrontendRouteSeedData(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IdentityServiceDbContext>();

            try
            {
                // 检查是否已经存在路由分组
                if (await context.Set<RouteGroup>().AnyAsync())
                {
                    Console.WriteLine("路由分组数据已存在，跳过初始化");
                    return;
                }

                Console.WriteLine("开始初始化路由分组和前端路由数据...");

                // 创建路由分组
                var routeGroups = await CreateRouteGroups(context);
                
                // 创建前端路由
                await CreateFrontendRoutes(context, routeGroups);
                
                Console.WriteLine("路由分组和前端路由数据初始化完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"路由数据初始化失败：{ex.Message}");
                throw;
            }
        }

        private static async Task<Dictionary<string, RouteGroup>> CreateRouteGroups(IdentityServiceDbContext context)
        {
            var routeGroups = new Dictionary<string, RouteGroup>
            {
                ["DASHBOARD"] = new RouteGroup
                {
                    Id = Guid.NewGuid(),
                    Name = "仪表盘",
                    Code = "DASHBOARD",
                    Description = "系统仪表盘和数据概览",
                    Icon = "Odometer",
                    Sort = 1,
                    GroupType = RouteGroupType.Frontend,
                    IsEnabled = true,
                    CreateTime = DateTime.UtcNow
                },
                ["SYSTEM"] = new RouteGroup
                {
                    Id = Guid.NewGuid(),
                    Name = "系统管理",
                    Code = "SYSTEM",
                    Description = "系统基础管理功能",
                    Icon = "Setting",
                    Sort = 2,
                    GroupType = RouteGroupType.Frontend,
                    IsEnabled = true,
                    CreateTime = DateTime.UtcNow
                },
                ["CONTENT"] = new RouteGroup
                {
                    Id = Guid.NewGuid(),
                    Name = "内容管理",
                    Code = "CONTENT",
                    Description = "内容发布和管理",
                    Icon = "Document",
                    Sort = 3,
                    GroupType = RouteGroupType.Frontend,
                    IsEnabled = true,
                    CreateTime = DateTime.UtcNow
                },
                ["ANALYTICS"] = new RouteGroup
                {
                    Id = Guid.NewGuid(),
                    Name = "数据分析",
                    Code = "ANALYTICS",
                    Description = "数据统计和分析",
                    Icon = "PieChart",
                    Sort = 4,
                    GroupType = RouteGroupType.Frontend,
                    IsEnabled = true,
                    CreateTime = DateTime.UtcNow
                }
            };

            context.Set<RouteGroup>().AddRange(routeGroups.Values);
            await context.SaveChangesAsync();

            Console.WriteLine($"创建了 {routeGroups.Count} 个路由分组");
            return routeGroups;
        }

        private static async Task CreateFrontendRoutes(IdentityServiceDbContext context, Dictionary<string, RouteGroup> routeGroups)
        {
            var frontendRoutes = new List<FrontendRoute>();

            // 仪表盘路由
            var dashboardGroup = routeGroups["DASHBOARD"];
            frontendRoutes.Add(FrontendRoute.Create(
                path: "/dashboard",
                name: "Dashboard",
                component: "/dashboard/index.vue",
                title: "仪表盘",
                sort: 1,
                icon: "Odometer",
                isMenu: true,
                requiredPermission: "dashboard.view",
                routeGroupId: dashboardGroup.Id
            ));

            // 系统管理路由
            var systemGroup = routeGroups["SYSTEM"];
            
            // 用户管理
            frontendRoutes.Add(FrontendRoute.Create(
                path: "/system/users",
                name: "UserManagement",
                component: "/system/users/index.vue",
                title: "用户管理",
                sort: 1,
                icon: "User",
                isMenu: true,
                requiredPermission: "users.view",
                routeGroupId: systemGroup.Id
            ));

            // 角色管理
            frontendRoutes.Add(FrontendRoute.Create(
                path: "/system/roles",
                name: "RoleManagement",
                component: "/system/roles/index.vue",
                title: "角色管理",
                sort: 2,
                icon: "UserFilled",
                isMenu: true,
                requiredPermission: "roles.view",
                routeGroupId: systemGroup.Id
            ));

            // 权限管理
            frontendRoutes.Add(FrontendRoute.Create(
                path: "/system/permissions",
                name: "PermissionManagement",
                component: "/system/permissions/index.vue",
                title: "权限管理",
                sort: 3,
                icon: "Lock",
                isMenu: true,
                requiredPermission: "permissions.view",
                routeGroupId: systemGroup.Id
            ));

            // 菜单管理
            frontendRoutes.Add(FrontendRoute.Create(
                path: "/system/menus",
                name: "MenuManagement",
                component: "/system/menus/index.vue",
                title: "菜单管理",
                sort: 4,
                icon: "Menu",
                isMenu: true,
                requiredPermission: "menus.view",
                routeGroupId: systemGroup.Id
            ));

            // 内容管理路由
            var contentGroup = routeGroups["CONTENT"];
            
            // 文章管理
            frontendRoutes.Add(FrontendRoute.Create(
                path: "/content/articles",
                name: "ArticleManagement",
                component: "/content/articles/index.vue",
                title: "文章管理",
                sort: 1,
                icon: "Document",
                isMenu: true,
                requiredPermission: "articles.view",
                routeGroupId: contentGroup.Id
            ));

            // 分类管理
            frontendRoutes.Add(FrontendRoute.Create(
                path: "/content/categories",
                name: "CategoryManagement",
                component: "/content/categories/index.vue",
                title: "分类管理",
                sort: 2,
                icon: "Collection",
                isMenu: true,
                requiredPermission: "categories.view",
                routeGroupId: contentGroup.Id
            ));

            // 数据分析路由
            var analyticsGroup = routeGroups["ANALYTICS"];
            
            // 数据概览
            frontendRoutes.Add(FrontendRoute.Create(
                path: "/analytics/overview",
                name: "AnalyticsOverview",
                component: "/analytics/overview/index.vue",
                title: "数据概览",
                sort: 1,
                icon: "PieChart",
                isMenu: true,
                requiredPermission: "analytics.overview.view",
                routeGroupId: analyticsGroup.Id
            ));

            // 用户分析
            frontendRoutes.Add(FrontendRoute.Create(
                path: "/analytics/users",
                name: "UserAnalytics",
                component: "/analytics/users/index.vue",
                title: "用户分析",
                sort: 2,
                icon: "User",
                isMenu: true,
                requiredPermission: "analytics.users.view",
                routeGroupId: analyticsGroup.Id
            ));

            context.Set<FrontendRoute>().AddRange(frontendRoutes);
            await context.SaveChangesAsync();

            Console.WriteLine($"创建了 {frontendRoutes.Count} 个前端路由");
        }
    }
}
