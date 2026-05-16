using MediatR;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Users.Commands.User;

/// <summary>
/// 获取用户登录历史查询命令
/// </summary>
public record GetUserLoginHistoryQuery(
    Guid? UserId = null,
    string? IpAddress = null,
    string? Status = null,
    DateTime? StartTime = null,
    DateTime? EndTime = null,
    int PageIndex = 1,
    int PageSize = 10
) : IRequest<UserLoginHistoryResult>;

/// <summary>
/// 获取指定用户最近登录历史查询命令
/// </summary>
public record GetUserRecentLoginHistoryQuery(
    Guid UserId,
    int Count = 10
) : IRequest<Result<List<UserLoginHistoryDto>>>;

/// <summary>
/// 获取登录统计信息查询命令
/// </summary>
public record GetUserLoginStatisticsQuery(
    Guid UserId
) : IRequest<Result<UserLoginStatisticsDto>>;

/// <summary>
/// 用户登录统计DTO
/// </summary>
public class UserLoginStatisticsDto
{
    /// <summary>
    /// 总登录次数
    /// </summary>
    public int TotalLoginCount { get; set; }
    
    /// <summary>
    /// 成功登录次数
    /// </summary>
    public int SuccessfulLoginCount { get; set; }
    
    /// <summary>
    /// 失败登录次数
    /// </summary>
    public int FailedLoginCount { get; set; }
    
    /// <summary>
    /// 首次登录时间
    /// </summary>
    public DateTime? FirstLoginTime { get; set; }
    
    /// <summary>
    /// 最后登录时间
    /// </summary>
    public DateTime? LastLoginTime { get; set; }
    
    /// <summary>
    /// 常用登录IP列表
    /// </summary>
    public List<string> FrequentIpAddresses { get; set; } = new();
    
    /// <summary>
    /// 常用设备列表
    /// </summary>
    public List<string> FrequentDevices { get; set; } = new();
    
    /// <summary>
    /// 最近7天登录次数
    /// </summary>
    public int Recent7DaysLoginCount { get; set; }
    
    /// <summary>
    /// 最近30天登录次数
    /// </summary>
    public int Recent30DaysLoginCount { get; set; }
}