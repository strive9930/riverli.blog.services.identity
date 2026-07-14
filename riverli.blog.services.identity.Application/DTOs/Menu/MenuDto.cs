// Application/DTOs/MenuDto.cs

using System.ComponentModel.DataAnnotations;
using riverli.blog.services.identity.Domain.Enums;

namespace riverli.blog.services.identity.Application.DTOs;

public record MenuDto(
    Guid Id,
    string Name,
    string Title,
    string Path,
    string Component,
    string Icon,
    Guid? ParentId,
    int SortOrder,
    int Type
);
public record CreateOrUpdateMenuDto
{
    [Required] [StringLength(50)] public string Name { get; init; } = string.Empty;
    [StringLength(50)] public string Title { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public string Component { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public Guid? ParentId { get; init; }
    public int SortOrder { get; init; }
    [Required] public MenuType Type { get; init; } 
}

/// <summary>
/// 菜单树节点 DTO，包含子节点集合
/// </summary>
// public record MenuTreeDto(
//     Guid Id,
//     string Name,
//     string Path,
//     string Component,
//     string Permission,
//     string Icon,
//     Guid? ParentId,
//     int SortOrder,
//     int Type,
//     List<MenuTreeDto> Children
// );
/// <summary>
/// 树形菜单 DTO (使用 class 以支持动态挂载子节点)
/// </summary>
public class MenuTreeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Component { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public int SortOrder { get; set; }
    public int Type { get; set; } 
    
    // 核心：子菜单集合
    public List<MenuTreeDto> Children { get; set; } = new List<MenuTreeDto>();
}