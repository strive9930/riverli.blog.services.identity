using RiverLi.DDD.Core.Domain.Common;

namespace RiverLi.Blog.Identity.Domain.Entities
{
    /// <summary>
    /// 用户登录历史记录实体
    /// </summary>
    public sealed class UserLoginHistory : BaseEntity<Guid>, IAggregateRoot
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public Guid UserId { get; set; }
        
        /// <summary>
        /// 登录时间
        /// </summary>
        public DateTime LoginTime { get; set; }
        
        /// <summary>
        /// 登录IP地址
        /// </summary>
        public string IpAddress { get; set; } = string.Empty;
        
        /// <summary>
        /// 登录地点（城市/地区）
        /// </summary>
        public string? Location { get; set; }
        
        /// <summary>
        /// 设备信息
        /// </summary>
        public string DeviceInfo { get; set; } = string.Empty;
        
        /// <summary>
        /// 浏览器信息
        /// </summary>
        public string? BrowserInfo { get; set; }
        
        /// <summary>
        /// 操作系统
        /// </summary>
        public string? OperatingSystem { get; set; }
        
        /// <summary>
        /// 登录状态（Success/Failed）
        /// </summary>
        public string Status { get; set; } = string.Empty;
        
        /// <summary>
        /// 失败原因（如果登录失败）
        /// </summary>
        public string? FailureReason { get; set; }
        
        /// <summary>
        /// 用户代理字符串
        /// </summary>
        public string? UserAgent { get; set; }
        
        /// <summary>
        /// 会话ID
        /// </summary>
        public string? SessionId { get; set; }
        
        /// <summary>
        /// 是否为首次登录
        /// </summary>
        public bool IsFirstLogin { get; set; }
        
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; } = DateTime.UtcNow;
        
        // 导航属性
        public ApplicationUser User { get; set; } = null!;
    }
}