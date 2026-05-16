using MediatR;
using RiverLi.Blog.Identity.Application.Users.Queries.GetAllUsers;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Users.Queries.GetUserById;

/// <summary>
/// 根据用户 ID 获取用户详情查询
/// </summary>
public record GetUserByIdQuery(
    Guid UserId
) : IRequest<Result<UserDetailDto>>;

/// <summary>
/// 用户详情 DTO
/// </summary>
public class UserDetailDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? NickName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime? CreateTime { get; set; }
    public DateTime? UpdateTime { get; set; }
    public List<RoleDto> Roles { get; set; } = new();
}
