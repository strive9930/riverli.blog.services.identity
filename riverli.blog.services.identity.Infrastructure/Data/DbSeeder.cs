// Infrastructure/Data/DbSeeder.cs

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using riverli.blog.services.identity.Domain.Entities;
using riverli.blog.services.identity.Domain.Enums;

namespace riverli.blog.services.identity.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // 1. 创建超级管理员角色
        const string adminRoleName = "Admin";
        if (!await roleManager.RoleExistsAsync(adminRoleName))
        {
            await roleManager.CreateAsync(new AppRole { Name = adminRoleName, Description = "超级管理员" });
        }

        var adminRole = await roleManager.FindByNameAsync(adminRoleName);

        // 2. 创建初始用户
        const string adminEmail = "admin@riverli.blog";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new AppUser
                { UserName = "admin", Email = adminEmail, RealName = "System Admin", EmailConfirmed = true };
            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded) await userManager.AddToRoleAsync(adminUser, adminRoleName);
        }

        // 3. 查询现有菜单，仅添加缺失的菜单
        var existingMenus = await dbContext.Menus.ToDictionaryAsync(m => m.Name, m => m);
        var newMenus = new List<Menu>();
        var menuIdMap = new Dictionary<string, Guid>();

        // 将已存在的菜单 ID 加入映射
        foreach (var kvp in existingMenus)
            menuIdMap[kvp.Key] = kvp.Value.Id;

        // 辅助方法：按名称获取菜单 ID（已存在则复用，不存在则新建并加入待添加列表）
        Guid GetOrCreateMenuId(string name, string? parentName, string title, string path,
            string component, string icon, int sortOrder, MenuType type)
        {
            if (menuIdMap.TryGetValue(name, out var existingId))
                return existingId;

            Guid? parentId = null;
            if (parentName != null && menuIdMap.TryGetValue(parentName, out var pid))
                parentId = pid;

            var menu = new Menu
            {
                Id = Guid.NewGuid(),
                ParentId = parentId,
                Name = name,
                Title = title,
                Path = path,
                Component = component,
                Icon = icon,
                SortOrder = sortOrder,
                Type = type
            };
            menuIdMap[name] = menu.Id;
            newMenus.Add(menu);
            return menu.Id;
        }

        // 定义并检查所有菜单（先父后子，确保父级 ID 优先解析）
        // -- 系统管理 --
        var systemMenuId = GetOrCreateMenuId("System", null, "系统管理", "/system", "Layout", "Setting", 1, MenuType.Directory);
        var userMenuId = GetOrCreateMenuId("User", "System", "用户管理", "user", "system/user/index", "User", 1, MenuType.Page);
        var roleMenuId = GetOrCreateMenuId("Role", "System", "角色管理", "role", "system/role/index", "Lock", 2, MenuType.Page);
        var menuMenuId = GetOrCreateMenuId("Menu", "System", "菜单管理", "menu", "system/menu/index", "Menu", 3, MenuType.Page);
        var apiResourceMenuId = GetOrCreateMenuId("ApiResource", "System", "API资源管理", "apiresource", "system/apiresource/index", "Link", 4, MenuType.Page);

        // -- 博客管理 --
        var blogMenuId = GetOrCreateMenuId("Blog", null, "博客管理", "/blog", "Layout", "Document", 2, MenuType.Directory);
        var articleMenuId = GetOrCreateMenuId("Article", "Blog", "文章管理", "article", "blog/article/index", "Edit", 1, MenuType.Page);
        var categoryMenuId = GetOrCreateMenuId("Category", "Blog", "分类管理", "category", "blog/category/index", "Connection", 2, MenuType.Page);
        var tagMenuId = GetOrCreateMenuId("Tag", "Blog", "标签管理", "tag", "blog/tag/index", "Collection", 3, MenuType.Page);
        var commentMenuId = GetOrCreateMenuId("Comment", "Blog", "评论审核", "comment", "blog/comment/index", "ChatDotSquare", 4, MenuType.Page);
        var friendLinkMenuId = GetOrCreateMenuId("FriendLink", "Blog", "友链管理", "friendlink", "blog/friendlink/index", "Link", 5, MenuType.Page);
        var messageMenuId = GetOrCreateMenuId("Message", "Blog", "留言管理", "message", "blog/message/index", "ChatDotSquare", 6, MenuType.Page);
        var recordMenuId = GetOrCreateMenuId("Record", "Blog", "动态管理", "record", "blog/record/index", "Notebook", 7, MenuType.Page);

        // -- 站点配置 --
        var configMenuId = GetOrCreateMenuId("Config", null, "站点配置", "/portal", "Layout", "Operation", 3, MenuType.Directory);
        var siteNavMenuId = GetOrCreateMenuId("SiteNavigation", "Config", "前台导航管理", "siteNavigation", "blog/siteNavigation/index", "Guide", 1, MenuType.Page);
        var mediaMenuId = GetOrCreateMenuId("Media", "Config", "媒体资源库", "media", "blog/media/index", "Picture", 2, MenuType.Page);

        // -- 任务调度 --
        var schedulerMenuId = GetOrCreateMenuId("Scheduler", null, "任务调度", "/scheduler", "Layout", "Odometer", 4, MenuType.Directory);
        var quartzMenuId = GetOrCreateMenuId("Quartz", "Scheduler", "Quartz", "quartz", "scheduler/quartz/index", "Timer", 1, MenuType.Page);

        // -- 配置中心 --
        var consulMenuId = GetOrCreateMenuId("ConsulCenter", null, "配置中心", "/consul", "Layout", "Setting", 5, MenuType.Directory);
        var consulConfigMenuId = GetOrCreateMenuId("ConsulConfig", "ConsulCenter", "配置管理", "config", "consul/config/index", "Document", 1, MenuType.Page);
        var consulConfigGroupMenuId = GetOrCreateMenuId("ConsulConfigGroup", "ConsulCenter", "配置组管理", "configGroup", "consul/configGroup/index", "FolderOpened", 2, MenuType.Page);

        // 批量插入缺失的菜单
        if (newMenus.Count > 0)
        {
            dbContext.Menus.AddRange(newMenus);
            await dbContext.SaveChangesAsync();
        }

        // 4. 查询现有 RoleMenu 绑定，仅添加缺失的权限映射
        if (adminRole != null)
        {
            var allMenuIds = new[]
            {
                systemMenuId, userMenuId, roleMenuId, menuMenuId, apiResourceMenuId,
                blogMenuId, articleMenuId, categoryMenuId, tagMenuId, commentMenuId,
                friendLinkMenuId, messageMenuId, recordMenuId,
                configMenuId, siteNavMenuId, mediaMenuId,
                schedulerMenuId, quartzMenuId,
                consulMenuId, consulConfigMenuId, consulConfigGroupMenuId
            };

            var existingRoleMenuIds = await dbContext.RoleMenus
                .Where(rm => rm.RoleId == adminRole.Id)
                .Select(rm => rm.MenuId)
                .ToListAsync();

            var existingSet = new HashSet<Guid>(existingRoleMenuIds);
            var missingRoleMenus = allMenuIds
                .Where(mid => !existingSet.Contains(mid))
                .Select(mid => new RoleMenu { RoleId = adminRole.Id, MenuId = mid })
                .ToList();

            if (missingRoleMenus.Count > 0)
            {
                dbContext.RoleMenus.AddRange(missingRoleMenus);
                await dbContext.SaveChangesAsync();
            }
        }

        // ✂️ 第 5 步和第 6 步 (关于 ApiResource 和 RoleApi 的播种) 已经全部删除！
    }
}