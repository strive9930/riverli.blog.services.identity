using MediatR;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Users.Queries.GetAllUsers;

/// <summary>
/// 获取所有用户查询（分页）
/// </summary>
public record GetAllUsersQuery(
    string? Keyword = null,
    bool? IsEnabled = null,
    int PageIndex = 1,
    int PageSize = 10
) : IRequest<PagedResult<UserDto>>;

/// <summary>
/// 用户 DTO
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? NickName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime? CreateTime { get; set; }
    public DateTime? LastLoginTime { get; set; }
    public List<RoleDto> Roles { get; set; } = new();
    public DateTime? UpdateTime { get; set; }
}

/// <summary>
/// 角色 DTO
/// </summary>
public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
}
