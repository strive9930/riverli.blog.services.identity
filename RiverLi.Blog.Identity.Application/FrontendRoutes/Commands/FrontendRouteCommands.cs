using MediatR;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.FrontendRoutes.Commands;

/// <summary>
/// 创建前端路由的命令
/// </summary>
public record CreateFrontendRouteCommand(
    string Path,
    string Name,
    string Component,
    string Title,
    string? Icon = null,
    int Sort = 0,
    Guid? ParentId = null,
    bool IsMenu = true,
    bool IsEnabled = true,
    string? RequiredPermission = null,
    string? Description = null,
    string? Meta = null,
    Guid? RouteGroupId = null
) : IRequest<Result<Guid>>;

/// <summary>
/// 更新前端路由的命令
/// </summary>
public record UpdateFrontendRouteCommand(
    Guid Id,
    string? Path = null,
    string? Name = null,
    string? Component = null,
    string? Title = null,
    string? Icon = null,
    int? Sort = null,
    Guid? ParentId = null,
    bool? IsMenu = null,
    bool? IsEnabled= null,
    string? RequiredPermission = null,
    string? Description = null,
    string? Meta = null,
    Guid? RouteGroupId = null
) : IRequest<Result<bool>>;

/// <summary>
/// 删除前端路由的命令
/// </summary>
public record DeleteFrontendRouteCommand(
    Guid Id
) : IRequest<Result<bool>>;

/// <summary>
/// 获取前端路由详情的查询命令
/// </summary>
public record GetFrontendRouteByIdQuery(
    Guid Id
) : IRequest<Result<FrontendRoute>>;

/// <summary>
/// 获取所有前端路由的查询命令
/// </summary>
public record GetAllFrontendRoutesQuery : IRequest<Result<List<FrontendRoute>>>;

/// <summary>
/// 获取启用的前端路由的查询命令
/// </summary>
public record GetEnabledFrontendRoutesQuery : IRequest<Result<List<FrontendRoute>>>;

/// <summary>
/// 分页获取前端路由的查询命令
/// </summary>
public record GetPagedFrontendRoutesQuery(
    string? Keyword = null,
    bool? IsEnabled = null,
    Guid? RouteGroupId = null,
    int PageIndex = 1,
    int PageSize = 10
) : IRequest<PagedResult<FrontendRoute>>;

/// <summary>
/// 获取菜单树结构的查询命令
/// </summary>
public record GetMenuTreeQuery(
    List<string> UserPermissions // 用户拥有的权限列表
) : IRequest<Result<List<FrontendRoute>>>;

/// <summary>
/// 为前端路由关联后端路由的命令
/// </summary>
public record AssociateBackendRoutesCommand(
    Guid FrontendRouteId,
    List<Guid> BackendRouteIds
) : IRequest<Result<bool>>;