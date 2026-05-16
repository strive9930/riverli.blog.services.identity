using MediatR;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.SysRoutes.Commands;

/// <summary>
/// 创建系统路由的命令
/// </summary>
public record CreateSysRouteCommand(
    string Path,                   // 对应 SysRoute.Path
    string Method,                 // 对应 SysRoute.Method
    string RequiredPermission = "",// 对应 SysRoute.RequiredPermission
    bool IsEnabled = true,         // 对应 SysRoute.IsEnabled
    string ServiceName = "",       // 对应 SysRoute.ServiceName
    Guid? RouteGroupId = null,     // 路由分组 ID
    Guid? FrontendRouteId = null   // 关联的前端路由 ID
) : IRequest<Result<Guid>>; // 返回新创建的路由的 ID

/// <summary>
/// 更新系统路由的命令
/// </summary>
public record UpdateSysRouteCommand(
    Guid Id,
    string Path,
    string Method,
    string RequiredPermission = "",
    bool IsEnabled = true,
    string ServiceName = "",
    Guid? RouteGroupId = null,
    Guid? FrontendRouteId = null
) : IRequest<Result<bool>>;

/// <summary>
/// 删除系统路由的命令
/// </summary>
public record DeleteSysRouteCommand(
    Guid Id
) : IRequest<Result<bool>>;

/// <summary>
/// 批量删除系统路由的命令
/// </summary>
public record BatchDeleteSysRoutesCommand(
    List<Guid> RouteIds
) : IRequest<Result<int>>;

/// <summary>
/// 启用系统路由的命令
/// </summary>
public record EnableSysRouteCommand(
    Guid Id
) : IRequest<Result<string>>;

/// <summary>
/// 禁用系统路由的命令
/// </summary>
public record DisableSysRouteCommand(
    Guid Id
) : IRequest<Result<string>>;

/// <summary>
/// 获取所有活跃路由的查询
/// </summary>
public record GetActiveRoutesQuery : IRequest<Result<List<SysRoute>>>;

/// <summary>
/// 根据 ID 获取路由的查询
/// </summary>
public record GetRouteByIdQuery(
    Guid Id
) : IRequest<Result<SysRoute>>;
