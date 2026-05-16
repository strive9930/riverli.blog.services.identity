namespace RiverLi.Blog.Identity.Application.Common.Interfaces;

/// <summary>
/// 登录历史服务
/// </summary>
public interface ILoginHistoryService
{
    /// <summary>
    /// 记录用户登录历史
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="ipAddress">IP地址</param>
    /// <param name="userAgent">用户代理</param>
    /// <param name="status">登录状态</param>
    /// <param name="failureReason">失败原因（如果登录失败）</param>
    /// <param name="sessionId">会话ID</param>
    Task RecordLoginHistoryAsync(
        Guid userId, 
        string ipAddress, 
        string userAgent, 
        string status, 
        string? failureReason = null,
        string? sessionId = null);
}