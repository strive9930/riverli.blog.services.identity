using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RiverLi.DDD.Core.Application.Common.Models;
using RiverLi.Blog.Identity.Application.Common.Models;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.Blog.Identity.Application.Common.Interfaces;
using RiverLi.Blog.Identity.Application.Common.Services;

namespace RiverLi.Blog.Identity.Application.Auth.Commands.Login
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
            // 4. 生成 JWT Token
            var token = _tokenService.GenerateToken(user, roles);
            var expiration = DateTime.Now.AddMinutes(60); // 需与 JwtTokenService 中逻辑一致

            // 5. 记录成功的登录历史
            await RecordSuccessfulLogin(user.Id, token);

            return Result<AuthResponse>.SuccessResult(new AuthResponse(
                token,
                expiration,
                user.Id,
                user.NickName));
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
                    sessionId: token.Substring(0, Math.Min(32, token.Length))); // 使用 token 前 32 位作为 session ID
            }
            catch (Exception ex)
            {
                // 记录登录历史失败不影响主流程
                Console.WriteLine($"记录成功登录历史失败：{ex.Message}");
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
                // 记录登录历史失败不影响主流程
                Console.WriteLine($"记录失败登录历史失败：{ex.Message}");
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
    }
}
