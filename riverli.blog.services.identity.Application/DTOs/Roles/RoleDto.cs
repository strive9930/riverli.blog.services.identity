using System.ComponentModel.DataAnnotations;

namespace riverli.blog.services.identity.Application.DTOs.Roles;

public record RoleDto(Guid Id, string Name, string Description);

public record CreateRoleDto
{
    [Required]
    [StringLength(50)]
    public string Name { get; init; } = string.Empty;

    [StringLength(200)]
    public string Description { get; init; } = string.Empty;
}

public record UpdateRoleDto
{
    [Required]
    [StringLength(50)]
    public string Name { get; init; } = string.Empty;

    [StringLength(200)]
    public string Description { get; init; } = string.Empty;
}

public record AssignRoleMenusRequest
{
    [Required]
    public List<Guid> MenuIds { get; init; } = new();
}

public record AssignRoleApisRequest
{
    [Required]
    public List<Guid> ApiIds { get; init; } = new();
}
