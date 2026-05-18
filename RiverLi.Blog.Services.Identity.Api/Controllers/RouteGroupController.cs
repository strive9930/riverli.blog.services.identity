using Microsoft.AspNetCore.Mvc;
using MediatR;
using RiverLi.Blog.Identity.Application.RouteGroups.Commands;
using RiverLi.Blog.Infrastructure.Shared.Controllers;

namespace RiverLi.Blog.Identity.Api.Controllers;

/// <summary>
/// 路由分组管理控制器
/// 提供路由分组的完整CRUD和树形结构查询
/// </summary>
[ApiController]
[Route("api/route-groups")]
public class RouteGroupController : BaseApiController
{
    private readonly IMediator _mediator;

    public RouteGroupController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region crud
    /// <summary>
    /// 获取所有路由分组（支持筛选）
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetRouteGroups(
        [FromQuery] int? groupType = null,
        [FromQuery] bool? isEnabled = null,
        [FromQuery] string? keyword = null)
    {
        var query = new GetAllRouteGroupsQuery(groupType, isEnabled, keyword);
        var result = await _mediator.Send(query);
        return result.Success ? Success(result.Data) : Fail(result.Message);
    }

    /// <summary>
    /// 分页获取路由分组列表
    /// 支持分组类型、状态、关键词筛选和分页
    /// </summary>
    [HttpGet("list/paged")]
    public async Task<IActionResult> GetRouteGroupsPaged(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? groupType = null,
        [FromQuery] bool? isEnabled = null,
        [FromQuery] string? keyword = null,
        [FromQuery] string? sortBy = "Sort",
        [FromQuery] bool sortDesc = false)
    {
        try
        {
            var query = new GetPagedRouteGroupsQuery(pageIndex, pageSize, groupType, isEnabled, keyword, sortBy, sortDesc);
            var result = await _mediator.Send(query);
            
            return result.Success ? Success(result.Data, result.TotalCount,result.PageIndex,result.PageSize) : Fail(result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "获取路由分组分页列表时发生错误", error = ex.Message });
        }
    }

    /// <summary>
    /// 获取路由分组树结构
    /// </summary>
    [HttpGet("tree")]
    public async Task<IActionResult> GetRouteGroupTree(
        [FromQuery] int? groupType = null,
        [FromQuery] bool? isEnabled = null)
    {
        var query = new GetRouteGroupTreeQuery(groupType, isEnabled);
        var result = await _mediator.Send(query);
        return result.Success ? Success(result.Data) : Fail(result.Message);
    }

    /// <summary>
    /// 根据ID获取路由分组
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRouteGroup(Guid id)
    {
        var query = new GetRouteGroupByIdQuery(id);
        var result = await _mediator.Send(query);
        return result.Success ? Success(result.Data) : Fail(result.Message);
    }

    /// <summary>
    /// 创建路由分组
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateRouteGroup([FromBody] CreateRouteGroupRequest request)
    {
        var command = new CreateRouteGroupCommand(
            request.Name,
            request.Code,
            request.Description,
            request.Icon,
            request.Sort,
            request.ParentId,
            request.IsEnabled,
            request.GroupType,
            request.RequiredPermission
        );
        var result = await _mediator.Send(command);
        return result.Success ? Success(result.Data) : Fail(result.Message);
    }

    /// <summary>
    /// 更新路由分组
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateRouteGroup(Guid id, [FromBody] UpdateRouteGroupRequest request)
    {
        var command = new UpdateRouteGroupCommand(
            id,
            request.Name,
            request.Code,
            request.Description,
            request.Icon,
            request.Sort,
            request.ParentId,
            request.IsEnabled,
            request.GroupType,
            request.RequiredPermission
        );
        var result = await _mediator.Send(command);
        return result.Success ? Success() : Fail(result.Message);
    }

    /// <summary>
    /// 删除路由分组
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteRouteGroup(Guid id)
    {
        var command = new DeleteRouteGroupCommand(id);
        var result = await _mediator.Send(command);
        return result.Success ? Success() : Fail(result.Message);
    }

    #endregion

    #region 批量操作

    /// <summary>
    /// 批量删除路由分组
    /// </summary>
    [HttpDelete("batch")]
    public async Task<IActionResult> BatchDeleteRouteGroups([FromBody] BatchRouteGroupOperationRequest request)
    {
        try
        {
            var command = new BatchDeleteRouteGroupsCommand(request.RouteGroupIds);
            var result = await _mediator.Send(command);
            
            return result.Success ? Success(result.Message) : Fail(result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "批量删除路由分组时发生错误", error = ex.Message });
        }
    }

    /// <summary>
    /// 批量更新路由分组状态
    /// </summary>
    [HttpPut("batch/status")]
    public async Task<IActionResult> BatchUpdateRouteGroupStatus([FromBody] BatchUpdateRouteGroupStatusCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            
            return result.Success ? Success(result.Message) : Fail(result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "批量更新路由分组状态时发生错误", error = ex.Message });
        }
    }

    /// <summary>
    /// 启用路由分组
    /// </summary>
    [HttpPut("{id:guid}/enable")]
    public async Task<IActionResult> EnableRouteGroup(Guid id)
    {
        try
        {
            var command = new EnableRouteGroupCommand(id);
            var result = await _mediator.Send(command);
            
            return result.Success ? Success(result.Message) : Fail(result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "启用路由分组时发生错误", error = ex.Message });
        }
    }

    /// <summary>
    /// 禁用路由分组
    /// </summary>
    [HttpPut("{id:guid}/disable")]
    public async Task<IActionResult> DisableRouteGroup(Guid id)
    {
        try
        {
            var command = new DisableRouteGroupCommand(id);
            var result = await _mediator.Send(command);
            
            return result.Success ? Success(result.Message) : Fail(result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "禁用路由分组时发生错误", error = ex.Message });
        }
    }
    #endregion
}

// DTO 定义
public class CreateRouteGroupRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int Sort { get; set; } = 0;
    public Guid? ParentId { get; set; }
    public bool IsEnabled { get; set; } = true;
    public int GroupType { get; set; } = 1;
    public string? RequiredPermission { get; set; }
}

public class UpdateRouteGroupRequest
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int? Sort { get; set; }
    public Guid? ParentId { get; set; }
    public bool? IsEnabled { get; set; }
    public int? GroupType { get; set; }
    public string? RequiredPermission { get; set; }
}
