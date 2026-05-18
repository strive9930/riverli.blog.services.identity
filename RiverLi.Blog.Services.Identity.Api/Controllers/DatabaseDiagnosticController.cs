using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.Blog.Infrastructure.Shared.Controllers;

namespace RiverLi.Blog.Identity.Api.Controllers
{
    /// <summary>
    /// 数据库诊断控制器
    /// 用于检查数据库状态和数据完整性
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DatabaseDiagnosticController : BaseApiController
    {
        private readonly IdentityServiceDbContext _context;

        public DatabaseDiagnosticController(IdentityServiceDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 获取数据库诊断报告（仅管理员可用）
        /// </summary>
        [HttpGet("report")]
        [AllowAnonymous] // 临时允许匿名访问以便调试
        public async Task<IActionResult> GetDatabaseReport()
        {
            var report = new
            {
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                
                // 1. 管理员角色检查
                adminRole = await CheckAdminRole(),
                
                // 2. 管理员用户检查
                adminUser = await CheckAdminUser(),
                
                // 3. 菜单数据检查
                menus = await CheckMenus(),
                
                // 4. 权限数据检查
                permissions = await CheckPermissions(),
                
                // 5. 用户 - 角色关系检查
                userRoleRelations = await CheckUserRoleRelations(),
                
                // 6. 角色 - 权限关系检查
                rolePermissionRelations = await CheckRolePermissionRelations()
            };

            return Success(report);
        }

        private async Task<object> CheckAdminRole()
        {
            var adminRole = await _context.Set<RiverLi.Blog.Identity.Domain.Entities.ApplicationRole>()
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Name == "Admin");

            if (adminRole == null)
            {
                return new { exists = false, message = "Admin 角色不存在" };
            }

            return new
            {
                exists = true,
                id = adminRole.Id,
                permissionCount = adminRole.Permissions.Count,
                permissions = adminRole.Permissions.Take(5).Select(p => p.Code).ToList()
            };
        }

        private async Task<object> CheckAdminUser()
        {
            var adminUser = await _context.Users
                .Include(u => u.Roles)
                .Include(u => u.Permissions)
                .FirstOrDefaultAsync(u => u.Email == "admin@example.com");

            if (adminUser == null)
            {
                return new { exists = false, message = "管理员账号不存在" };
            }

            return new
            {
                exists = true,
                id = adminUser.Id,
                email = adminUser.Email,
                roleCount = adminUser.Roles.Count,
                directPermissionCount = adminUser.Permissions.Count
            };
        }

        private async Task<object> CheckMenus()
        {
            var allMenus = await _context.Menus.ToListAsync();
            var enabledMenus = await _context.Menus.Where(m => m.IsEnabled && m.IsVisible).ToListAsync();

            return new
            {
                totalCount = allMenus.Count,
                enabledCount = enabledMenus.Count,
                hasData = allMenus.Any(),
                sampleMenus = allMenus.Take(5).Select(m => new
                {
                    id = m.Id,
                    name = m.Name,
                    title = m.Title,
                    path = m.Path,
                    isEnabled = m.IsEnabled,
                    isVisible = m.IsVisible,
                    requiredPermission = m.RequiredPermission ?? "无"
                }).ToList()
            };
        }

        private async Task<object> CheckPermissions()
        {
            var allPermissions = await _context.Permissions.ToListAsync();

            return new
            {
                totalCount = allPermissions.Count,
                hasData = allPermissions.Any(),
                samplePermissions = allPermissions.Take(10).Select(p => p.Code).ToList()
            };
        }

        private async Task<object> CheckUserRoleRelations()
        {
            // 检查 AspNetUserRoles 表的数据
            var userRoleCount = await _context.UserRoles.CountAsync();
            
            // 获取管理员用户的角色关系
            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "admin@example.com");
            if (adminUser == null)
            {
                return new { exists = false, message = "管理员用户不存在" };
            }

            var adminRoles = await _context.UserRoles
                .Where(ur => ur.UserId == adminUser.Id)
                .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r)
                .ToListAsync();

            return new
            {
                totalUserRoleRecords = userRoleCount,
                adminUserId = adminUser.Id,
                adminRoleCount = adminRoles.Count,
                adminRoles = adminRoles.Select(r => new { r.Id, r.Name, r.Code }).ToList()
            };
        }

        private async Task<object> CheckRolePermissionRelations()
        {
            // 检查 RolePermissions 表的数据
            var rolePermissionCount = await _context.Set<RiverLi.Blog.Identity.Domain.Entities.ApplicationRole>()
                .SelectMany(r => r.Permissions)
                .CountAsync();

            // 获取 Admin 角色的权限关系
            var adminRole = await _context.Set<RiverLi.Blog.Identity.Domain.Entities.ApplicationRole>()
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Name == "Admin");

            if (adminRole == null)
            {
                return new { exists = false, message = "Admin 角色不存在" };
            }

            return new
            {
                totalRolePermissionRecords = rolePermissionCount,
                adminRoleId = adminRole.Id,
                adminRolePermissionCount = adminRole.Permissions.Count,
                adminRolePermissions = adminRole.Permissions.Take(10).Select(p => new { p.Id, p.Code, p.Name }).ToList()
            };
        }
    }
}
