using System.ComponentModel.DataAnnotations;

namespace riverli.blog.services.identity.Application.DTOs.Users;

#region 1. DTOs
public record UserDto(Guid Id, string Username, string RealName, string Email, bool IsActive, DateTime CreatedAt);

public record CreateUserDto
{
    [Required] [StringLength(50)] public string Username { get; init; } = string.Empty;
    [Required] [StringLength(100, MinimumLength = 6)] public string Password { get; init; } = string.Empty;
    public string RealName { get; init; } = string.Empty;
    [EmailAddress] public string Email { get; init; } = string.Empty;
}

public record UpdateUserDto
{
    public string RealName { get; init; } = string.Empty;
    [EmailAddress] public string Email { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}

public record AssignUserRolesRequest
{
    [Required]
    public List<string> RoleIds { get; init; } = new();
}
#endregion
