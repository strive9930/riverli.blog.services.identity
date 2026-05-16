using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;

namespace RiverLi.Blog.Identity.Api
{
    /// <summary>
    /// 基础种子数据 - 创建管理员账号和角色
    /// </summary>
    public static class SeedData
    {
        public static async Task EnsureSeedData(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var dbContext = scope.ServiceProvider.GetRequiredService<IdentityServiceDbContext>();

            // 创建角色
            string[] roles = { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new ApplicationRole 
                    { 
                        Name = role, 
                        Description = $"{role} role",
                        Code = role.ToLower(),
                        IsEnabled = true,
                        CreateTime = DateTime.UtcNow
                    });
                    Console.WriteLine($"创建角色：{role}");
                }
            }

            // 创建默认管理员账号
            var adminEmail = "admin@example.com";
            var adminPassword = "Admin123!";
            
            if (await dbContext.Users.FirstOrDefaultAsync(u => u.Email == adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    NickName = "超级管理员",
                    IsEnabled = true,
                    CreateTime = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                
                if (result.Succeeded)
                {
                    // 将管理员分配到 Admin 角色
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine($"创建管理员账号：{adminEmail}, ID: {adminUser.Id}");
                }
                else
                {
                    Console.WriteLine($"创建管理员账号失败：{string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
