using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Users.Commands.User;

/// <summary>
/// 用户登录历史DTO
/// </summary>
public class UserLoginHistoryDto
{
    /// <summary>
    /// 记录ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// 用户邮箱
    /// </summary>
    public string UserEmail { get; set; } = string.Empty;
    
    /// <summary>
    /// 用户昵称
    /// </summary>
    public string? UserNickName { get; set; }
    
    /// <summary>
    /// 登录时间
    /// </summary>
    public DateTime LoginTime { get; set; }
    
    /// <summary>
    /// 登录IP地址
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// 登录地点
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
    /// 登录状态
    /// </summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>
    /// 失败原因
    /// </summary>
    public string? FailureReason { get; set; }
    
    /// <summary>
    /// 是否为首次登录
    /// </summary>
    public bool IsFirstLogin { get; set; }
}

/// <summary>
/// 用户登录历史查询结果
/// </summary>
public class UserLoginHistoryResult
{
    public int TotalCount { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public List<UserLoginHistoryDto> Items { get; set; } = new();
    
    public UserLoginHistoryResult(int totalCount, int pageIndex, int pageSize, List<UserLoginHistoryDto> items)
    {
        TotalCount = totalCount;
        PageIndex = pageIndex;
        PageSize = pageSize;
        Items = items;
    }
}