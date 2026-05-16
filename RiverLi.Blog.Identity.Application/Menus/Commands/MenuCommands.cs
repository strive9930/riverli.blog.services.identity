using MediatR;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Menus.Commands;

/// <summary>
/// 创建菜单的命令
/// </summary>
public record CreateMenuCommand(
    string Name,
    string Title,
    string? Path = null,
    string? Icon = null,
    int Sort = 0,
    Guid? ParentId = null,
    bool IsEnabled = true,
    bool IsVisible = true,
    int MenuType = 1, // MenuType.MenuItem
    Guid? FrontendRouteId = null,
    Guid? MenuGroupId = null,
    string? RequiredPermission = null,
    string? Description = null,
    string? Meta = null
) : IRequest<Result<Guid>>;

/// <summary>
/// 更新菜单的命令
/// </summary>
public record UpdateMenuCommand(
    Guid Id,
    string Name,
    string Title,
    string? Path = null,
    string? Icon = null,
    int Sort = 0,
    Guid? ParentId = null,
    bool IsEnabled = true,
    bool IsVisible = true,
    int MenuType = 1,
    Guid? FrontendRouteId = null,
    Guid? MenuGroupId = null,
    string? RequiredPermission = null,
    string? Description = null,
    string? Meta = null
) : IRequest<Result<bool>>;

/// <summary>
/// 删除菜单的命令
/// </summary>
public record DeleteMenuCommand(
    Guid Id
) : IRequest<Result<bool>>;

/// <summary>
/// 获取菜单详情的查询命令
/// </summary>
public record GetMenuByIdQuery(
    Guid Id
) : IRequest<Result<MenuDto>>;

/// <summary>
/// 获取所有菜单的查询命令
/// </summary>
public record GetAllMenusQuery(
    Guid? MenuGroupId = null,
    int? MenuType = null,
    bool? IsEnabled = null,
    bool? IsVisible = null,
    string? Keyword = null
) : IRequest<Result<List<MenuDto>>>;

/// <summary>
/// 分页获取菜单的查询命令
/// </summary>
public record GetPagedMenusQuery(
    int PageIndex = 1,
    int PageSize = 10,
    Guid? MenuGroupId = null,
    int? MenuType = null,
    bool? IsEnabled = null,
    bool? IsVisible = null,
    string? Keyword = null,
    string SortBy = "Sort",
    bool SortDesc = false
) : IRequest<PagedResult<MenuDto>>;

/// <summary>
/// 获取菜单树结构的查询命令
/// </summary>
public record GetMenuTreeQuery(
    Guid? MenuGroupId = null,
    int? MenuType = null,
    bool? IsEnabled = null,
    bool? IsVisible = null,
    List<string>? UserPermissions = null
) : IRequest<Result<List<MenuTreeNodeDto>>>;

/// <summary>
/// 创建菜单组的命令
/// </summary>
public record CreateMenuGroupCommand(
    string Name,
    string Code,
    string? Description = null,
    string? Icon = null,
    int Sort = 0,
    bool IsEnabled = true
) : IRequest<Result<Guid>>;

/// <summary>
/// 更新菜单组的命令
/// </summary>
public record UpdateMenuGroupCommand(
    Guid Id,
    string? Name = null,
    string? Code = null,
    string? Description = null,
    string? Icon = null,
    int Sort = 0,
    bool IsEnabled = true
) : IRequest<Result<bool>>;

/// <summary>
/// 删除菜单组的命令
/// </summary>
public record DeleteMenuGroupCommand(
    Guid Id
) : IRequest<Result<bool>>;

/// <summary>
/// 获取所有菜单组的查询命令
/// </summary>
public record GetAllMenuGroupsQuery(
    bool? IsEnabled = null,
    string? Keyword = null
) : IRequest<Result<List<MenuGroupDto>>>;

/// <summary>
/// 分页获取菜单组的查询命令
/// </summary>
public record GetPagedMenuGroupsQuery(
    string? Keyword = null,
    int PageIndex = 1,
    int PageSize = 10,
    bool? IsEnabled = null,
    string SortBy = "Sort",
    bool SortDesc = false
) : IRequest<PagedResult<MenuGroupDto>>;

/// <summary>
/// 更新菜单排序的命令
/// </summary>
public record UpdateMenuSortCommand(
    List<Guid> MenuIds
) : IRequest<Result<bool>>;

/// <summary>
/// 批量删除菜单的命令
/// </summary>
public record BatchDeleteMenusCommand(
    List<Guid> MenuIds
) : IRequest<Result<bool>>;

/// <summary>
/// 批量更新菜单状态的命令
/// </summary>
public record BatchUpdateMenuStatusCommand(
    List<Guid> MenuIds,
    bool IsEnabled
) : IRequest<Result<bool>>;

/// <summary>
/// 菜单 DTO
/// 用于展示菜单的完整信息
/// </summary>
public class MenuDto
{
    /// <summary>
    /// 菜单唯一标识符
    /// </summary>
    public Guid Id { get; set; }
    
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
    public string? Path { get; set; }
    
    /// <summary>
    /// 菜单图标
    /// </summary>
    public string? Icon { get; set; }
    
    /// <summary>
    /// 排序值
    /// </summary>
    public int Sort { get; set; }
    
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }
    
    /// <summary>
    /// 是否可见
    /// </summary>
    public bool IsVisible { get; set; }
    
    /// <summary>
    /// 父级菜单 ID
    /// </summary>
    public Guid? ParentId { get; set; }
    
    /// <summary>
    /// 菜单类型
    /// </summary>
    public int MenuType { get; set; }
    
    /// <summary>
    /// 关联的前端路由 ID
    /// </summary>
    public Guid? FrontendRouteId { get; set; }
    
    /// <summary>
    /// 所属菜单组 ID
    /// </summary>
    public Guid? MenuGroupId { get; set; }
    
    /// <summary>
    /// 所需权限码
    /// </summary>
    public string? RequiredPermission { get; set; }
    
    /// <summary>
    /// 描述信息
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 元数据（JSON 格式）
    /// </summary>
    public string? Meta { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreateTime { get; set; }
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }
    
    /// <summary>
    /// 父级菜单信息
    /// </summary>
    public MenuDto? Parent { get; set; }
    
    /// <summary>
    /// 子菜单列表
    /// </summary>
    public List<MenuDto> Children { get; set; } = new();
    
    /// <summary>
    /// 所属菜单组信息
    /// </summary>
    public MenuGroupDto? MenuGroup { get; set; }
    
    /// <summary>
    /// 前端路由路径
    /// </summary>
    public string? FrontendRoutePath { get; set; }
}

/// <summary>
/// 菜单树节点 DTO
/// 用于展示菜单的树形结构
/// </summary>
public class MenuTreeNodeDto
{
    /// <summary>
    /// 菜单唯一标识符
    /// </summary>
    public Guid Id { get; set; }
    
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
    public string? Path { get; set; }
    
    /// <summary>
    /// 菜单图标
    /// </summary>
    public string? Icon { get; set; }
    
    /// <summary>
    /// 排序值
    /// </summary>
    public int Sort { get; set; }
    
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }
    
    /// <summary>
    /// 是否可见
    /// </summary>
    public bool IsVisible { get; set; }
    
    /// <summary>
    /// 菜单类型
    /// </summary>
    public int MenuType { get; set; }
    
    /// <summary>
    /// 父级菜单 ID
    /// </summary>
    public Guid? ParentId { get; set; }
    
    /// <summary>
    /// 子节点列表
    /// </summary>
    public List<MenuTreeNodeDto> Children { get; set; } = new();
}

/// <summary>
/// 菜单组 DTO
/// 用于展示菜单组的完整信息
/// </summary>
public class MenuGroupDto
{
    /// <summary>
    /// 菜单组唯一标识符
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// 菜单组名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 菜单组编码
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// 描述信息
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 图标
    /// </summary>
    public string? Icon { get; set; }
    
    /// <summary>
    /// 排序值
    /// </summary>
    public int Sort { get; set; }
    
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreateTime { get; set; }
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }
    
    /// <summary>
    /// 该分组下的菜单列表
    /// </summary>
    public List<MenuDto> Menus { get; set; } = new();
    
    /// <summary>
    /// 菜单总数
    /// </summary>
    public int MenuCount { get; set; }
}