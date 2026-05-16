using MediatR;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Auth.Queries.GetCurrentUserInfo;

/// <summary>
/// 获取当前用户信息查询
/// </summary>
public class GetCurrentUserInfoQuery : IRequest<Result<UserInfoDto>>
{
    public Guid UserId { get; set; }
    
    public GetCurrentUserInfoQuery(Guid userId)
    {
        UserId = userId;
    }
}

/// <summary>
/// 用户信息 DTO
/// </summary>
public class UserInfoDto
{
    /// <summary>
    /// 用户 ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// 昵称
    /// </summary>
    public string? NickName { get; set; }
    
    /// <summary>
    /// 邮箱
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// 头像 URL
    /// </summary>
    public string? Avatar { get; set; }
    
    /// <summary>
    /// 用户角色列表
    /// </summary>
    public List<string> Roles { get; set; } = new();
    
    /// <summary>
    /// 用户权限列表
    /// </summary>
    public List<string> Permissions { get; set; } = new();
    
    /// <summary>
    /// 是否为管理员
    /// </summary>
    public bool IsAdmin { get; set; }
    
    /// <summary>
    /// 最后登录时间
    /// </summary>
    public DateTime? LastLoginTime { get; set; }
    
    /// <summary>
    /// 账户创建时间
    /// </summary>
    public DateTime? CreatedAt { get; set; }
}
