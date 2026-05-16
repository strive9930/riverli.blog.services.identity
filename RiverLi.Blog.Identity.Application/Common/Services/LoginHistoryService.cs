using Microsoft.AspNetCore.Http;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Application.Common.Interfaces;

namespace RiverLi.Blog.Identity.Application.Common.Services
{
    

    /// <summary>
    /// 登录历史服务实现
    /// </summary>
    public class LoginHistoryService : ILoginHistoryService
    {
        private readonly IdentityServiceDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoginHistoryService(
            IdentityServiceDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task RecordLoginHistoryAsync(
            Guid userId, 
            string ipAddress, 
            string userAgent, 
            string status, 
            string? failureReason = null,
            string? sessionId = null)
        {
            try
            {
                // 解析用户代理信息
                var deviceInfo = ParseDeviceInfo(userAgent);
                var browserInfo = ParseBrowserInfo(userAgent);
                var osInfo = ParseOperatingSystem(userAgent);
                var location = await GetLocationFromIpAsync(ipAddress);

                // 检查是否为首次登录
                var isFirstLogin = !await _context.UserLoginHistories
                    .AsNoTracking()
                    .AnyAsync(ulh => ulh.UserId == userId);

                var loginHistory = new UserLoginHistory
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    LoginTime = DateTime.UtcNow,
                    IpAddress = ipAddress,
                    Location = location,
                    DeviceInfo = deviceInfo,
                    BrowserInfo = browserInfo,
                    OperatingSystem = osInfo,
                    Status = status,
                    FailureReason = failureReason,
                    UserAgent = userAgent,
                    SessionId = sessionId,
                    IsFirstLogin = isFirstLogin,
                    CreateTime = DateTime.UtcNow
                };

                _context.UserLoginHistories.Add(loginHistory);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // 记录日志但不抛出异常，避免影响登录流程
                Console.WriteLine($"记录登录历史失败: {ex.Message}");
            }
        }

        private string ParseDeviceInfo(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return "Unknown Device";

            // 简单的设备检测
            if (userAgent.Contains("Mobile") || userAgent.Contains("Android") || userAgent.Contains("iPhone"))
                return "Mobile Device";
            else if (userAgent.Contains("Tablet"))
                return "Tablet";
            else
                return "Desktop";
        }

        private string ParseBrowserInfo(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return "Unknown Browser";

            if (userAgent.Contains("Chrome") && !userAgent.Contains("Edg"))
                return "Chrome";
            else if (userAgent.Contains("Firefox"))
                return "Firefox";
            else if (userAgent.Contains("Safari") && !userAgent.Contains("Chrome"))
                return "Safari";
            else if (userAgent.Contains("Edg"))
                return "Edge";
            else if (userAgent.Contains("Opera"))
                return "Opera";
            else
                return "Other Browser";
        }

        private string ParseOperatingSystem(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return "Unknown OS";

            if (userAgent.Contains("Windows"))
                return "Windows";
            else if (userAgent.Contains("Macintosh") || userAgent.Contains("Mac OS"))
                return "macOS";
            else if (userAgent.Contains("Linux"))
                return "Linux";
            else if (userAgent.Contains("Android"))
                return "Android";
            else if (userAgent.Contains("iPhone") || userAgent.Contains("iPad"))
                return "iOS";
            else
                return "Other OS";
        }

        private async Task<string?> GetLocationFromIpAsync(string ipAddress)
        {
            try
            {
                // 这里可以集成IP地理位置服务
                // 例如使用 ip-api.com, ipinfo.io 等免费服务
                // 为了简化，这里返回固定值
                if (ipAddress == "::1" || ipAddress == "127.0.0.1")
                    return "Localhost";
                
                // 实际项目中应该调用外部API获取地理位置
                return "Unknown Location";
            }
            catch
            {
                return "Location Lookup Failed";
            }
        }
    }
}