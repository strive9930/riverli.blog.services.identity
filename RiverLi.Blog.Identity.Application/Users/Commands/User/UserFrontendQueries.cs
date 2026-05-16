using MediatR;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Users.Commands.User;

/// <summary>
/// 获取用户信息
/// </summary>
public record GetUserInfoQuery(Guid UserId) : IRequest<Result<UserInfoDto>>;

/// <summary>
/// 获取用户菜单（树形结构）
/// </summary>
public record GetUserMenusQuery(Guid UserId) : IRequest<Result<List<MenuTreeDto>>>;

/// <summary>
/// 获取用户权限码列表
/// </summary>
public record GetUserPermissionCodesQuery(Guid UserId) : IRequest<Result<List<string>>>;

/// <summary>
/// 获取当前用户角色列表
/// </summary>
public record GetCurrentUserRolesQuery(Guid UserId) : IRequest<Result<List<RoleDto>>>;

/// <summary>
/// 获取仪表盘统计数据
/// </summary>
public record GetDashboardStatsQuery(Guid UserId) : IRequest<Result<DashboardStatsDto>>;

/// <summary>
/// 用户信息 DTO
/// 用于展示用户的完整信息
/// </summary>
public class UserInfoDto
{
    /// <summary>
    /// 用户 ID（字符串格式）
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// 邮箱地址
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// 昵称
    /// </summary>
    public string? Nickname { get; set; }
    
    /// <summary>
    /// 头像 URL 地址
    /// </summary>
    public string? AvatarUrl { get; set; }
    
    /// <summary>
    /// 电话号码
    /// </summary>
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreateTime { get; set; }
    
    /// <summary>
    /// 用户角色列表
    /// </summary>
    public List<RoleDto> Roles { get; set; } = new();
}

/// <summary>
/// 菜单树 DTO
/// 用于展示菜单的树形结构
/// </summary>
public class MenuTreeDto
{
    /// <summary>
    /// 菜单 ID（字符串格式）
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// 菜单名称（英文标识）
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 菜单标题（显示名称）
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 菜单路径
    /// </summary>
    public string Path { get; set; } = string.Empty;
    
    /// <summary>
    /// 菜单图标
    /// </summary>
    public string? Icon { get; set; }
    
    /// <summary>
    /// 排序值
    /// </summary>
    public int Sort { get; set; }
    
    /// <summary>
    /// 菜单类型
    /// </summary>
    public string MenuType { get; set; } = string.Empty;
    
    /// <summary>
    /// 所需权限码
    /// </summary>
    public string? RequiredPermission { get; set; }
    
    /// <summary>
    /// 子菜单列表
    /// </summary>
    public List<MenuTreeDto> Children { get; set; } = new();
}

/// <summary>
/// 仪表盘统计数据 DTO
/// 用于展示系统统计信息
/// </summary>
public class DashboardStatsDto
{
    /// <summary>
    /// 用户总数
    /// </summary>
    public int TotalUsers { get; set; }
    
    /// <summary>
    /// 文章总数（需要从文章服务获取）
    /// </summary>
    public int TotalArticles { get; set; }
    
    /// <summary>
    /// 总访问量（需要从访问统计服务获取）
    /// </summary>
    public int TotalVisits { get; set; }
    
    /// <summary>
    /// 最近活动列表
    /// </summary>
    public List<object> RecentActivities { get; set; } = new();
}
