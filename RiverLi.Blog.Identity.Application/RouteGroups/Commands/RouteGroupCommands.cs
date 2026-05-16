using MediatR;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.RouteGroups.Commands;

/// <summary>
/// 创建路由分组的命令
/// </summary>
public record CreateRouteGroupCommand(
    string Name,
    string Code,
    string? Description = null,
    string? Icon = null,
    int Sort = 0,
    Guid? ParentId = null,
    bool IsEnabled = true,
    int GroupType = 1, // RouteGroupType.Frontend
    string? RequiredPermission = null
) : IRequest<Result<Guid>>;

/// <summary>
/// 更新路由分组的命令
/// </summary>
public record UpdateRouteGroupCommand(
    Guid Id,
    string? Name = null,
    string? Code = null,
    string? Description = null,
    string? Icon = null,
    int? Sort = null,
    Guid? ParentId = null,
    bool? IsEnabled = null,
    int? GroupType = null,
    string? RequiredPermission = null
) : IRequest<Result<bool>>;

/// <summary>
/// 删除路由分组的命令
/// </summary>
public record DeleteRouteGroupCommand(
    Guid Id
) : IRequest<Result<bool>>;

/// <summary>
/// 获取路由分组详情的查询命令
/// </summary>
public record GetRouteGroupByIdQuery(
    Guid Id
) : IRequest<Result<RouteGroupDto>>;

/// <summary>
/// 获取所有路由分组的查询命令
/// </summary>
public record GetAllRouteGroupsQuery(
    int? GroupType = null,
    bool? IsEnabled = null,
    string? Keyword = null
) : IRequest<Result<List<RouteGroupDto>>>;

/// <summary>
/// 获取分组树结构的查询命令
/// </summary>
public record GetRouteGroupTreeQuery(
    int? GroupType = null,
    bool? IsEnabled = null
) : IRequest<Result<List<RouteGroupTreeNodeDto>>>;

/// <summary>
/// 分页获取路由分组的查询命令
/// </summary>
public record GetPagedRouteGroupsQuery(
    int PageIndex = 1,
    int PageSize = 10,
    int? GroupType = null,
    bool? IsEnabled = null,
    string? Keyword = null,
    string? SortBy = "Sort",
    bool SortDesc = false
) : IRequest<PagedResult<RouteGroupDto>>;

/// <summary>
/// 批量删除路由分组的命令
/// </summary>
public record BatchDeleteRouteGroupsCommand(
    List<Guid> RouteGroupIds
) : IRequest<Result<int>>;

/// <summary>
/// 批量更新路由分组状态的命令
/// </summary>
public record BatchUpdateRouteGroupStatusCommand(
    List<Guid> RouteGroupIds,
    bool IsEnabled
) : IRequest<Result<int>>;

/// <summary>
/// 启用路由分组的命令
/// </summary>
public record EnableRouteGroupCommand(
    Guid Id
) : IRequest<Result<string>>;

/// <summary>
/// 禁用路由分组的命令
/// </summary>
public record DisableRouteGroupCommand(
    Guid Id
) : IRequest<Result<string>>;

/// <summary>
/// 路由分组 DTO
/// 用于展示路由分组的完整信息
/// </summary>
public class RouteGroupDto
{
    /// <summary>
    /// 路由分组唯一标识符
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// 分组名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 分组编码
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
    /// 父级分组 ID
    /// </summary>
    public Guid? ParentId { get; set; }
    
    /// <summary>
    /// 分组类型（1=前端，2=后端）
    /// </summary>
    public int GroupType { get; set; }
    
    /// <summary>
    /// 所需权限码
    /// </summary>
    public string? RequiredPermission { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime? CreateTime { get; set; }
    
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdateTime { get; set; }
    
    /// <summary>
    /// 父级分组信息
    /// </summary>
    public RouteGroupDto? Parent { get; set; }
    
    /// <summary>
    /// 子分组列表
    /// </summary>
    public List<RouteGroupDto> Children { get; set; } = new();
    
    /// <summary>
    /// 前端路由数量
    /// </summary>
    public int FrontendRouteCount { get; set; }
    
    /// <summary>
    /// 后端路由数量
    /// </summary>
    public int BackendRouteCount { get; set; }
}

/// <summary>
/// 路由分组树节点 DTO
/// 用于展示路由分组的树形结构
/// </summary>
public class RouteGroupTreeNodeDto
{
    /// <summary>
    /// 分组唯一标识符
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// 分组名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 分组编码
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
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
    /// 父级分组 ID
    /// </summary>
    public Guid? ParentId { get; set; }
    
    /// <summary>
    /// 子节点列表
    /// </summary>
    public List<RouteGroupTreeNodeDto> Children { get; set; } = new();
}

/// <summary>
/// 批量切换路由分组启用状态的命令
/// </summary>
public record BatchToggleRouteGroupsCommand(
    List<Guid> Ids,
    bool IsEnabled
) : IRequest<Result<int>>;

/// <summary>
/// 批量操作请求DTO
/// </summary>
public class BatchRouteGroupOperationRequest
{
    public List<Guid> RouteGroupIds { get; set; } = new();
}