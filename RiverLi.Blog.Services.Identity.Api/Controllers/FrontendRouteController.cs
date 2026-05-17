using Microsoft.AspNetCore.Mvc;
using MediatR;
using RiverLi.Blog.Identity.Application.FrontendRoutes.Commands;
using RiverLi.Blog.Identity.Domain.Entities;
//using RiverLi.Blog.Infrastructure.Shared.Controllers;

namespace RiverLi.Blog.Identity.Api.Controllers;

/// <summary>
/// 前端路由管理控制器
/// 提供前端路由的完整CRUD、树形结构查询及后端路由关联
/// </summary>
[ApiController]
[Route("api/frontend-routes")]
public class FrontendRouteController : BaseApiController
{
    private readonly IMediator _mediator;

    public FrontendRouteController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 获取所有前端路由
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetFrontendRoutes()
    {
        var query = new GetAllFrontendRoutesQuery();
        var result = await _mediator.Send(query);
        return result.Success ? Success(result.Data) : Fail(result.Message);
    }

    /// <summary>
    /// 分页获取前端路由列表
    /// </summary>
    [HttpGet("paged")]
    public async Task<IActionResult> GetPagedFrontendRoutes(
        [FromQuery] string? keyword = null,
        [FromQuery] bool? isEnabled = null,
        [FromQuery] Guid? routeGroupId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetPagedFrontendRoutesQuery(keyword, isEnabled, routeGroupId, page, pageSize);
        var result = await _mediator.Send(query);
        return result.Success ? Success(result.Data, result.TotalCount, result.PageIndex, result.PageSize) : Fail(result.Message);
    }

    /// <summary>
    /// 获取前端路由树结构
    /// </summary>
    [HttpGet("tree")]
    public async Task<IActionResult> GetFrontendRouteTree()
    {
        var query = new GetAllFrontendRoutesQuery();
        var result = await _mediator.Send(query);
        
        if (!result.Success) return Fail(result.Message);

        // 构建树形结构
        var routes = result.Data;
        var rootRoutes = routes?.Where(r => r.ParentId == null).OrderBy(r => r.Sort).ToList();
        
        foreach (var root in rootRoutes)
        {
            BuildRouteTree(root, routes);
        }

        return Success(rootRoutes);
    }

    private void BuildRouteTree(FrontendRoute parent, List<FrontendRoute> allRoutes)
    {
        // 递归构建树结构 - 在实际项目中由查询处理器处理
        var children = allRoutes.Where(r => r.ParentId == parent.Id).OrderBy(r => r.Sort).ToList();
        parent.Children = children;
        
        foreach (var child in children)
        {
            BuildRouteTree(child, allRoutes);
        }
    }

    /// <summary>
    /// 根据ID获取前端路由详情
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetFrontendRoute(Guid id)
    {
        var query = new GetFrontendRouteByIdQuery(id);
        var result = await _mediator.Send(query);
        return result.Success ? Success(result.Data) : Fail(result.Message);
    }

    /// <summary>
    /// 创建前端路由
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateFrontendRoute([FromBody] CreateFrontendRouteRequest request)
    {
        var command = new CreateFrontendRouteCommand(
            request.Path,
            request.Name,
            request.Component,
            request.Title,
            request.Icon,
            request.Sort,
            request.ParentId,
            request.IsMenu,
            request.IsEnabled,
            request.RequiredPermission,
            request.Description,
            request.Meta,
            request.RouteGroupId
        );
        var result = await _mediator.Send(command);
        return result.Success ? Success(result.Data) : Fail(result.Message);
    }

    /// <summary>
    /// 更新前端路由
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateFrontendRoute(Guid id, [FromBody] UpdateFrontendRouteRequest request)
    {
        var command = new UpdateFrontendRouteCommand(
            id,
            request.Path,
            request.Name,
            request.Component,
            request.Title,
            request.Icon,
            request.Sort,
            request.ParentId,
            request.IsMenu,
            request.IsEnabled,
            request.RequiredPermission,
            request.Description,
            request.Meta,
            request.RouteGroupId
        );
        var result = await _mediator.Send(command);
        return result.Success ? Success() : Fail(result.Message);
    }

    /// <summary>
    /// 删除前端路由
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteFrontendRoute(Guid id)
    {
        var command = new DeleteFrontendRouteCommand(id);
        var result = await _mediator.Send(command);
        return result.Success ? Success() : Fail(result.Message);
    }

    /// <summary>
    /// 为前端路由关联后端路由
    /// </summary>
    [HttpPost("{id:guid}/backend-routes")]
    public async Task<IActionResult> AssociateBackendRoutes(Guid id, [FromBody] AssociateBackendRoutesRequest request)
    {
        var command = new AssociateBackendRoutesCommand(id, request.BackendRouteIds);
        var result = await _mediator.Send(command);
        return result.Success ? Success() : Fail(result.Message);
    }

    /// <summary>
    /// 获取启用的菜单树（用于动态路由生成）
    /// </summary>
    [HttpGet("menu-tree")]
    public async Task<IActionResult> GetMenuTree([FromQuery] string permissions = "")
    {
        var userPermissions = permissions.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
        var query = new GetMenuTreeQuery(userPermissions);
        var result = await _mediator.Send(query);
        return result.Success ? Success(result.Data) : Fail(result.Message);
    }
}

// DTO 定义
public class CreateFrontendRouteRequest
{
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Component { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int Sort { get; set; } = 0;
    public Guid? ParentId { get; set; }
    public Guid? RouteGroupId { get; set; }
    public bool IsMenu { get; set; } = true;
    public bool IsEnabled { get; set; } = true;
    public string? RequiredPermission { get; set; }
    public string? Description { get; set; }
    public string? Meta { get; set; }
}

public class UpdateFrontendRouteRequest
{
    public string? Path { get; set; }
    public string? Name { get; set; }
    public string? Component { get; set; }
    public string? Title { get; set; }
    public string? Icon { get; set; }
    public int? Sort { get; set; }
    public Guid? ParentId { get; set; }
    public Guid? RouteGroupId { get; set; }
    public bool? IsMenu { get; set; }
    public bool? IsEnabled { get; set; }
    public string? RequiredPermission { get; set; }
    public string? Description { get; set; }
    public string? Meta { get; set; }
}

public class AssociateBackendRoutesRequest
{
    public List<Guid> BackendRouteIds { get; set; } = new();
}