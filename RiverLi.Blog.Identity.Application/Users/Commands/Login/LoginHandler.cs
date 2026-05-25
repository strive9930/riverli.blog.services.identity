using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using RiverLi.DDD.Core.Application.Common.Models;
using RiverLi.Blog.Identity.Application.Common.Models;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.Blog.Identity.Application.Common.Interfaces;
using RiverLi.Blog.Identity.Application.Common.Services;

namespace RiverLi.Blog.Identity.Application.Users.Commands.Login
{
    /// <summary>
    /// 登录逻辑处理器
    /// </summary>
    public class LoginHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtTokenService _tokenService;
        private readonly IdentityServiceDbContext _context;
        private readonly ILoginHistoryService _loginHistoryService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoginHandler(
            UserManager<ApplicationUser> userManager,
            JwtTokenService tokenService,
            IdentityServiceDbContext context,
            ILoginHistoryService loginHistoryService,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _context = context;
            _loginHistoryService = loginHistoryService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // 1. 查找用户
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // 记录失败的登录尝试
                await RecordFailedLogin(null, "UserNotFound", request.Email);
                return Result<AuthResponse>.FailResult("用户不存在或密码错误");
            }

            // 2. 验证密码 (Identity 内部会进行哈希比对)
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
            {
                // 记录失败的登录尝试
                await RecordFailedLogin(user.Id, "InvalidPassword", request.Email);
                return Result<AuthResponse>.FailResult("用户不存在或密码错误");
            }

            // 3. 获取用户角色（用于放入 JWT Claim）
            var roles = await _userManager.GetRolesAsync(user);
            
            // 4. 获取这些角色对应的所有权限代码 (关键步骤)
            //通过 EF Core 联表查询 ApplicationRole -> Permissions
            var permissions = await _context.Roles
                .Where(r => roles.Contains(r.Name!))
                .SelectMany(r => r.Permissions)
                .Select(p => p.Code)
                .Distinct()
                .ToListAsync(cancellationToken);
            
            // 5. 生成 JWT Token
            var token = _tokenService.GenerateToken(user, roles);
            var expiration = DateTime.Now.AddMinutes(60); // 需与 JwtTokenService 中逻辑一致

            // 6. 构建角色信息列表
            var roleInfos = await _context.Roles
                .Where(r => roles.Contains(r.Name!))
                .Select(r => new RoleInfo
                {
                    Id = r.Id,
                    Name = r.Name ?? string.Empty,
                    Code = r.Code ?? string.Empty,
                    Description = r.Description
                })
                .ToListAsync(cancellationToken);
            
            // 7. 判断是否为管理员
            bool isAdmin = roles.Any(r => r == "Admin" || r == "Administrator");
            
            // 8. 获取用户的菜单树（仅后台管理菜单）
            var menus = await GetUserAdminMenus(user.Id, permissions, isAdmin, cancellationToken);

            // 9. 记录成功的登录历史
            await RecordSuccessfulLogin(user.Id, token);

            // 10. 构建完整的响应对象
            var response = new AuthResponse
            {
                Token = token,
                Expiration = expiration,
                UserId = user.Id,
                Nickname = user.NickName,
                Username = user.UserName ?? string.Empty,
                Email = user.Email,
                Avatar = user.AvatarUrl,
                Roles = roleInfos,
                Permissions = permissions.ToList(),
                Menus = menus,
                IsAdmin = isAdmin,
                CreatedAt = user.CreateTime
            };

            return Result<AuthResponse>.SuccessResult(response);
        }

        private async Task RecordSuccessfulLogin(Guid userId, string token)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var ipAddress = GetClientIpAddress(httpContext);
                var userAgent = httpContext?.Request.Headers["User-Agent"].ToString() ?? string.Empty;
                
                await _loginHistoryService.RecordLoginHistoryAsync(
                    userId, 
                    ipAddress, 
                    userAgent, 
                    "Success",
                    sessionId: token.Substring(0, Math.Min(32, token.Length))); // 使用token前32位作为session ID
            }
            catch (Exception ex)
            {
                Console.WriteLine($"记录成功登录历史失败: {ex.Message}");
            }
        }

        private async Task RecordFailedLogin(Guid? userId, string failureReason, string email)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var ipAddress = GetClientIpAddress(httpContext);
                var userAgent = httpContext?.Request.Headers["User-Agent"].ToString() ?? string.Empty;
                
                await _loginHistoryService.RecordLoginHistoryAsync(
                    userId ?? Guid.Empty, 
                    ipAddress, 
                    userAgent, 
                    "Failed",
                    failureReason: $"{failureReason}: {email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"记录失败登录历史失败: {ex.Message}");
            }
        }

        private string GetClientIpAddress(HttpContext? context)
        {
            if (context == null) return "Unknown IP";

            // 尝试从各种头部获取真实IP
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
        }

        /// <summary>
        /// 获取用户的后台管理菜单树
        /// </summary>
        private async Task<List<MenuTreeItem>> GetUserAdminMenus(Guid userId, List<string> permissions, bool isAdmin, CancellationToken cancellationToken)
        {
            try
            {
                // 获取所有启用的菜单
                var allMenus = await _context.Menus
                    .Where(m => m.IsEnabled && m.IsVisible)
                    .OrderBy(m => m.Sort)
                    .ToListAsync(cancellationToken);

                // 根据权限和角色过滤菜单
                var filteredMenus = allMenus.Where(m =>
                {
                    // 如果没有 RequiredPermission，所有人都可以访问
                    if (string.IsNullOrEmpty(m.RequiredPermission))
                    {
                        return true;
                    }

                    // 如果是后台管理菜单（通过 TargetAudience 判断），只有管理员可以访问
                    bool isAdminMenu = m.TargetAudience == RiverLi.Blog.Identity.Domain.Entities.MenuTarget.Admin;
                    if (isAdminMenu && !isAdmin)
                    {
                        return false;
                    }
                    
                    // 检查是否有对应权限
                    return permissions.Contains(m.RequiredPermission);
                }).ToList();

                // 构建树形结构
                return BuildMenuTree(filteredMenus, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取用户菜单失败：{ex.Message}");
                return new List<MenuTreeItem>();
            }
        }

        /// <summary>
        /// 构建菜单树形结构
        /// </summary>
        private List<MenuTreeItem> BuildMenuTree(List<RiverLi.Blog.Identity.Domain.Entities.Menu> menus, Guid? parentId)
        {
            var parentMenus = menus.Where(m => m.ParentId == parentId).ToList();
            var result = new List<MenuTreeItem>();

            foreach (var menu in parentMenus)
            {
                var menuItem = new MenuTreeItem
                {
                    Id = menu.Id.ToString(),
                    Name = menu.Name,
                    Title = menu.Title,
                    Path = menu.Path ?? string.Empty,
                    Icon = menu.Icon,
                    Sort = menu.Sort,
                    RequiredPermission = menu.RequiredPermission,
                    Children = BuildMenuTree(menus, menu.Id)
                };

                result.Add(menuItem);
            }

            return result;
        }
    }
}
