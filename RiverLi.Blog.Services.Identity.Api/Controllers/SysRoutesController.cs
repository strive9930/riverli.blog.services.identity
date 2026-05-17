using Microsoft.AspNetCore.Mvc;
using MediatR;
using RiverLi.Blog.Identity.Application.SysRoutes.Commands;
//using RiverLi.Blog.Infrastructure.Shared.Controllers;

namespace RiverLi.Blog.Identity.Api.Controllers;

/// <summary>
/// 系统路由管理控制器
/// 提供后端路由的完整 CRUD 操作
/// </summary>
[ApiController]
[Route("api/sys-routes")]
public class SysRoutesController : BaseApiController
{
    private readonly IMediator _mediator;

    public SysRoutesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 获取所有活跃的系统路由
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetActiveRoutes()
    {
        var query = new GetActiveRoutesQuery();
        var result = await _mediator.Send(query);
        return result.Success ? Success(result.Data) : Fail(result.Message);
    }

    /// <summary>
    /// 根据 ID 获取路由
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRoute(Guid id)
    {
        var query = new GetRouteByIdQuery(id);
        var result = await _mediator.Send(query);
        return result.Success ? Success(result.Data) : Fail(result.Message);
    }

    /// <summary>
    /// 创建系统路由
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateRoute([FromBody] CreateSysRouteRequest request)
    {
        var command = new CreateSysRouteCommand(
            request.Path,
            request.Method,
            request.RequiredPermission,
            request.IsEnabled,
            request.ServiceName,
            request.RouteGroupId,
            request.FrontendRouteId
        );
        var result = await _mediator.Send(command);
        return result.Success ? Success(result.Data) : Fail(result.Message);
    }

    /// <summary>
    /// 更新系统路由
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateRoute(Guid id, [FromBody] UpdateSysRouteRequest request)
    {
        var command = new UpdateSysRouteCommand(
            id,
            request.Path,
            request.Method,
            request.RequiredPermission,
            request.IsEnabled,
            request.ServiceName,
            request.RouteGroupId,
            request.FrontendRouteId
        );
        var result = await _mediator.Send(command);
        return result.Success ? Success() : Fail(result.Message);
    }

    /// <summary>
    /// 删除系统路由
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteRoute(Guid id)
    {
        var command = new DeleteSysRouteCommand(id);
        var result = await _mediator.Send(command);
        return result.Success ? Success() : Fail(result.Message);
    }

    /// <summary>
    /// 批量删除系统路由
    /// </summary>
    [HttpDelete("batch")]
    public async Task<IActionResult> BatchDeleteRoutes([FromBody] List<Guid> routeIds)
    {
        var command = new BatchDeleteSysRoutesCommand(routeIds);
        var result = await _mediator.Send(command);
        return result.Success ? Success(result.Message) : Fail(result.Message);
    }

    /// <summary>
    /// 启用系统路由
    /// </summary>
    [HttpPut("{id:guid}/enable")]
    public async Task<IActionResult> EnableRoute(Guid id)
    {
        var command = new EnableSysRouteCommand(id);
        var result = await _mediator.Send(command);
        return result.Success ? Success(result.Message) : Fail(result.Message);
    }

    /// <summary>
    /// 禁用系统路由
    /// </summary>
    [HttpPut("{id:guid}/disable")]
    public async Task<IActionResult> DisableRoute(Guid id)
    {
        var command = new DisableSysRouteCommand(id);
        var result = await _mediator.Send(command);
        return result.Success ? Success(result.Message) : Fail(result.Message);
    }
}

/// <summary>
/// 创建系统路由请求 DTO
/// </summary>
public class CreateSysRouteRequest
{
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string RequiredPermission { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public string ServiceName { get; set; } = string.Empty;
    public Guid? RouteGroupId { get; set; }
    public Guid? FrontendRouteId { get; set; }
}

/// <summary>
/// 更新系统路由请求 DTO
/// </summary>
public class UpdateSysRouteRequest
{
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string RequiredPermission { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public string ServiceName { get; set; } = string.Empty;
    public Guid? RouteGroupId { get; set; }
    public Guid? FrontendRouteId { get; set; }
}
