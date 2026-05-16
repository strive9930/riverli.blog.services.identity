using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;

namespace RiverLi.Blog.Identity.Api
{
    /// <summary>
    /// 后端路由种子数据初始化器
    /// 负责初始化后端 API 路由数据
    /// </summary>
    public static class BackendRouteSeeder
    {
        public static async Task EnsureBackendRouteSeedData(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IdentityServiceDbContext>();

            try
            {
                // 检查是否已经存在后端路由
                if (await context.SysRoutes.AnyAsync())
                {
                    Console.WriteLine("后端路由数据已存在，跳过初始化");
                    return;
                }

                Console.WriteLine("开始初始化后端路由数据...");

                // 创建后端路由（先创建路由分组）
                var routeGroups = await CreateRouteGroups(context);

                // 创建后端路由
                await CreateBackendRoutes(context, routeGroups);

                Console.WriteLine("后端路由数据初始化完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"后端路由数据初始化失败：{ex.Message}");
                throw;
            }
        }

        private static async Task<Dictionary<string, RouteGroup>> CreateRouteGroups(IdentityServiceDbContext context)
        {
            // 定义需要的后端路由分组 Codes
            var requiredBackendGroupCodes = new[] { "AUTH", "USER", "ROLE", "PERMISSION", "MENU", "ARTICLE", "CATEGORY" };
            
            // 获取所有现有的路由分组
            var existingGroups = await context.Set<RouteGroup>().ToListAsync();
            
            // 筛选出我们需要的后端路由分组
            var backendGroups = existingGroups
                .Where(g => requiredBackendGroupCodes.Contains(g.Code))
                .ToDictionary(g => g.Code, g => g);
            
            // 如果已经存在所有需要的分组，直接返回
            if (backendGroups.Count == requiredBackendGroupCodes.Length)
            {
                Console.WriteLine($"使用已存在的 {backendGroups.Count} 个后端路由分组");
                return backendGroups;
            }
            
            // 否则创建缺失的分组
            var routeGroups = new Dictionary<string, RouteGroup>();
            
            foreach (var code in requiredBackendGroupCodes)
            {
                if (backendGroups.ContainsKey(code))
                {
                    routeGroups[code] = backendGroups[code];
                }
                else
                {
                    var newGroup = CreateBackendGroup(code);
                    routeGroups[code] = newGroup;
                    context.Set<RouteGroup>().Add(newGroup);
                }
            }
            
            await context.SaveChangesAsync();
            Console.WriteLine($"创建了 {routeGroups.Count - backendGroups.Count} 个新的后端路由分组");
            return routeGroups;
        }
        
        private static RouteGroup CreateBackendGroup(string code)
        {
            return code switch
            {
                "AUTH" => new RouteGroup
                {
                    Id = Guid.NewGuid(),
                    Name = "认证授权",
                    Code = "AUTH",
                    Description = "用户认证和授权相关接口",
                    Icon = "Lock",
                    Sort = 1,
                    GroupType = RouteGroupType.Backend,
                    IsEnabled = true,
                    CreateTime = DateTime.UtcNow
                },
                "USER" => new RouteGroup
                {
                    Id = Guid.NewGuid(),
                    Name = "用户管理",
                    Code = "USER",
                    Description = "用户管理相关接口",
                    Icon = "User",
                    Sort = 2,
                    GroupType = RouteGroupType.Backend,
                    IsEnabled = true,
                    CreateTime = DateTime.UtcNow
                },
                "ROLE" => new RouteGroup
                {
                    Id = Guid.NewGuid(),
                    Name = "角色管理",
                    Code = "ROLE",
                    Description = "角色管理相关接口",
                    Icon = "UserFilled",
                    Sort = 3,
                    GroupType = RouteGroupType.Backend,
                    IsEnabled = true,
                    CreateTime = DateTime.UtcNow
                },
                "PERMISSION" => new RouteGroup
                {
                    Id = Guid.NewGuid(),
                    Name = "权限管理",
                    Code = "PERMISSION",
                    Description = "权限管理相关接口",
                    Icon = "Lock",
                    Sort = 4,
                    GroupType = RouteGroupType.Backend,
                    IsEnabled = true,
                    CreateTime = DateTime.UtcNow
                },
                "MENU" => new RouteGroup
                {
                    Id = Guid.NewGuid(),
                    Name = "菜单管理",
                    Code = "MENU",
                    Description = "菜单管理相关接口",
                    Icon = "Menu",
                    Sort = 5,
                    GroupType = RouteGroupType.Backend,
                    IsEnabled = true,
                    CreateTime = DateTime.UtcNow
                },
                "ARTICLE" => new RouteGroup
                {
                    Id = Guid.NewGuid(),
                    Name = "文章管理",
                    Code = "ARTICLE",
                    Description = "文章管理相关接口",
                    Icon = "Document",
                    Sort = 6,
                    GroupType = RouteGroupType.Backend,
                    IsEnabled = true,
                    CreateTime = DateTime.UtcNow
                },
                "CATEGORY" => new RouteGroup
                {
                    Id = Guid.NewGuid(),
                    Name = "分类管理",
                    Code = "CATEGORY",
                    Description = "分类管理相关接口",
                    Icon = "Collection",
                    Sort = 7,
                    GroupType = RouteGroupType.Backend,
                    IsEnabled = true,
                    CreateTime = DateTime.UtcNow
                },
                _ => throw new ArgumentException($"未知的后端路由分组代码：{code}")
            };
        }

        private static async Task CreateBackendRoutes(IdentityServiceDbContext context,
            Dictionary<string, RouteGroup> routeGroups)
        {
            var backendRoutes = new List<SysRoute>();

            // 认证授权路由
            var authGroup = routeGroups["AUTH"];
            backendRoutes.AddRange(new[]
            {
                CreateRoute("/api/auth/login", "POST", "用户登录", "Auth.Login", null, authGroup.Id),
                CreateRoute("/api/auth/register", "POST", "用户注册", "Auth.Register", null, authGroup.Id),
                CreateRoute("/api/auth/logout", "POST", "用户登出", "Auth.Logout", "auth.logout", authGroup.Id),
                CreateRoute("/api/auth/refresh", "POST", "刷新令牌", "Auth.RefreshToken", null, authGroup.Id),
            });

            // 用户管理路由
            var userGroup = routeGroups["USER"];
            backendRoutes.AddRange(new[]
            {
                CreateRoute("/api/users", "GET", "获取用户列表", "Users.List", "users.view", userGroup.Id),
                CreateRoute("/api/users/{id}", "GET", "获取用户详情", "Users.GetById", "users.view", userGroup.Id),
                CreateRoute("/api/users", "POST", "创建用户", "Users.Create", "users.create", userGroup.Id),
                CreateRoute("/api/users/{id}", "PUT", "更新用户", "Users.Update", "users.edit", userGroup.Id),
                CreateRoute("/api/users/{id}", "DELETE", "删除用户", "Users.Delete", "users.delete", userGroup.Id),
            });

            // 角色管理路由
            var roleGroup = routeGroups["ROLE"];
            backendRoutes.AddRange(new[]
            {
                CreateRoute("/api/roles", "GET", "获取角色列表", "Roles.List", "roles.view", roleGroup.Id),
                CreateRoute("/api/roles/{id}", "GET", "获取角色详情", "Roles.GetById", "roles.view", roleGroup.Id),
                CreateRoute("/api/roles", "POST", "创建角色", "Roles.Create", "roles.create", roleGroup.Id),
                CreateRoute("/api/roles/{id}", "PUT", "更新角色", "Roles.Update", "roles.edit", roleGroup.Id),
                CreateRoute("/api/roles/{id}", "DELETE", "删除角色", "Roles.Delete", "roles.delete", roleGroup.Id),
                CreateRoute("/api/roles/{id}/permissions", "GET", "获取角色权限", "Roles.GetPermissions", "roles.view",
                    roleGroup.Id),
                CreateRoute("/api/roles/{id}/permissions", "POST", "分配角色权限", "Roles.AssignPermissions", "roles.edit",
                    roleGroup.Id),
            });

            // 权限管理路由
            var permissionGroup = routeGroups["PERMISSION"];
            backendRoutes.AddRange(new[]
            {
                CreateRoute("/api/permissions", "GET", "获取权限列表", "Permissions.List", "permissions.view",
                    permissionGroup.Id),
                CreateRoute("/api/permissions/tree", "GET", "获取权限树", "Permissions.Tree", "permissions.view",
                    permissionGroup.Id),
                CreateRoute("/api/permissions", "POST", "创建权限", "Permissions.Create", "permissions.create",
                    permissionGroup.Id),
                CreateRoute("/api/permissions/{id}", "PUT", "更新权限", "Permissions.Update", "permissions.edit",
                    permissionGroup.Id),
                CreateRoute("/api/permissions/{id}", "DELETE", "删除权限", "Permissions.Delete", "permissions.delete",
                    permissionGroup.Id),
            });

            // 菜单管理路由
            var menuGroup = routeGroups["MENU"];
            backendRoutes.AddRange(new[]
            {
                CreateRoute("/api/menus", "GET", "获取菜单列表", "Menus.List", "menus.view", menuGroup.Id),
                CreateRoute("/api/menus/tree", "GET", "获取菜单树", "Menus.Tree", "menus.view", menuGroup.Id),
                CreateRoute("/api/menus", "POST", "创建菜单", "Menus.Create", "menus.create", menuGroup.Id),
                CreateRoute("/api/menus/{id}", "PUT", "更新菜单", "Menus.Update", "menus.edit", menuGroup.Id),
                CreateRoute("/api/menus/{id}", "DELETE", "删除菜单", "Menus.Delete", "menus.delete", menuGroup.Id),
            });

            // 文章管理路由
            var articleGroup = routeGroups["ARTICLE"];
            backendRoutes.AddRange(new[]
            {
                CreateRoute("/api/articles", "GET", "获取文章列表", "Articles.List", "articles.view", articleGroup.Id),
                CreateRoute("/api/articles/{id}", "GET", "获取文章详情", "Articles.GetById", "articles.view",
                    articleGroup.Id),
                CreateRoute("/api/articles", "POST", "创建文章", "Articles.Create", "articles.create", articleGroup.Id),
                CreateRoute("/api/articles/{id}", "PUT", "更新文章", "Articles.Update", "articles.edit", articleGroup.Id),
                CreateRoute("/api/articles/{id}", "DELETE", "删除文章", "Articles.Delete", "articles.delete",
                    articleGroup.Id),
            });

            // 分类管理路由
            var categoryGroup = routeGroups["CATEGORY"];
            backendRoutes.AddRange(new[]
            {
                CreateRoute("/api/categories", "GET", "获取分类列表", "Categories.List", "categories.view", categoryGroup.Id),
                CreateRoute("/api/categories/tree", "GET", "获取分类树", "Categories.Tree", "categories.view",
                    categoryGroup.Id),
                CreateRoute("/api/categories", "POST", "创建分类", "Categories.Create", "categories.create",
                    categoryGroup.Id),
                CreateRoute("/api/categories/{id}", "PUT", "更新分类", "Categories.Update", "categories.edit",
                    categoryGroup.Id),
                CreateRoute("/api/categories/{id}", "DELETE", "删除分类", "Categories.Delete", "categories.delete",
                    categoryGroup.Id),
            });

            context.SysRoutes.AddRange(backendRoutes);
            await context.SaveChangesAsync();

            Console.WriteLine($"创建了 {backendRoutes.Count} 个后端路由");
        }

        private static SysRoute CreateRoute(string path, string method, string description, string name,
            string? requiredPermission, Guid? routeGroupId)
        {
            return new SysRoute
            {
                Id = Guid.NewGuid(),
                Path = path,
                Method = method,
                RequiredPermission = requiredPermission ?? string.Empty,
                RouteGroupId = routeGroupId,
                ServiceName = name,
                CreateTime = DateTime.UtcNow
            };
        }
    }
}