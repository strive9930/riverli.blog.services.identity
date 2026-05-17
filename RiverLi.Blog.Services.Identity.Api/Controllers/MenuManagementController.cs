using Microsoft.AspNetCore.Mvc;
using MediatR;
using RiverLi.Blog.Identity.Application.Menus.Commands;
//using RiverLi.Blog.Infrastructure.Shared.Controllers;

namespace RiverLi.Blog.Identity.Api.Controllers;

/// <summary>
/// 菜单资源控制器
/// </summary>
[ApiController]
[Route("api/menus")]
public class MenusController : BaseApiController
{
    private readonly IMediator _mediator;

    public MenusController(IMediator mediator)
        => _mediator = mediator;

    #region 菜单 CRUD 操作

    /// <summary>
    /// 获取所有菜单列表
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMenus(
        [FromQuery] Guid? menuGroupId = null,
        [FromQuery] int? menuType = null,
        [FromQuery] bool? isEnabled = null,
        [FromQuery] bool? isVisible = null,
        [FromQuery] string? keyword = null)
    {
        var query = new GetAllMenusQuery(menuGroupId, menuType, isEnabled, isVisible, keyword);
        var result = await _mediator.Send(query);

        return result.Success ? Success(result.Data) : Fail(result.Message);
    }

    /// <summary>
    /// 分页获取菜单列表
    /// </summary>
    [HttpGet("paged")]
    public async Task<IActionResult> GetPagedMenus(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? menuGroupId = null,
        [FromQuery] int? menuType = null,
        [FromQuery] bool? isEnabled = null,
        [FromQuery] bool? isVisible = null,
        [FromQuery] string? keyword = null,
        [FromQuery] string sortBy = "Sort",
        [FromQuery] bool sortDesc = false)
    {
        var query = new GetPagedMenusQuery(pageIndex, pageSize, menuGroupId, menuType, isEnabled, isVisible, keyword,
            sortBy, sortDesc);
        var result = await _mediator.Send(query);
        return result.Success
            ? Success(result.Data, result.TotalCount, result.PageIndex, result.PageSize)
            : Fail(result.Message);
    }

    /// <summary>
    /// 获取菜单详情
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMenu(Guid id)
    {
        var query = new GetMenuByIdQuery(id);
        var result = await _mediator.Send(query);

        return result.Success ? Success(result.Data) : NotFound(result.Message);
    }

    /// <summary>
    /// 创建菜单
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateMenu([FromBody] CreateMenuCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Success ? Success(result.Data) : Fail(result.Message);
    }

    /// <summary>
    /// 更新菜单
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateMenu(Guid id, [FromBody] UpdateMenuCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("菜单ID不匹配");
        }

        var result = await _mediator.Send(command);
        return result.Success ? Success() : Fail(result.Message);
    }

    /// <summary>
    /// 删除菜单
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteMenu(Guid id)
    {
        var command = new DeleteMenuCommand(id);
        var result = await _mediator.Send(command);
        return result.Success ? Success() : Fail(result.Message);
    }

    /// <summary>
    /// 获取菜单树结构
    /// </summary>
    [HttpGet("tree")]
    public async Task<IActionResult> GetMenuTree()
    {
        var query = new GetMenuTreeQuery();
        var result = await _mediator.Send(query);
        return result.Success ? Success(result.Data) : Fail(result.Message);
    }

    #endregion

    #region 菜单组管理

    /// <summary>
    /// 获取所有菜单组
    /// </summary>
    [HttpGet("groups")]
    public async Task<IActionResult> GetMenuGroups()
    {
        var query = new GetAllMenuGroupsQuery();
        var result = await _mediator.Send(query);
        return result.Success ? Success(result.Data) : Fail(result.Message);
    }

    /// <summary>
    /// 获取菜单组分页
    /// </summary>
    [HttpGet("groups/paged")]
    public async Task<IActionResult> GetPageMenuGroups(
        [FromQuery] string? keyword = null,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isEnabled = null,
        [FromQuery] string sortBy = "Sort",
        [FromQuery] bool sortDesc = false)
    {
        var query = new GetPagedMenuGroupsQuery(keyword, pageIndex, pageSize,isEnabled, sortBy, sortDesc);
        var result = await _mediator.Send(query);
        return result.Success
            ? Success(result.Data, result.TotalCount, result.PageIndex, result.PageSize)
            : Fail(result.Message);
    }

    /// <summary>
    /// 创建菜单组
    /// </summary>
    [HttpPost("groups")]
    public async Task<IActionResult> CreateMenuGroup([FromBody] CreateMenuGroupCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Success ? Success(result.Data) : Fail(result.Message);
    }

    /// <summary>
    /// 更新菜单组
    /// </summary>
    [HttpPut("groups/{id:guid}")]
    public async Task<IActionResult> UpdateMenuGroup(Guid id, [FromBody] UpdateMenuGroupCommand command)
    {
        // 如果命令中没有指定 ID，使用 URL 中的 ID
        if (command.Id == Guid.Empty)
        {
            command = command with { Id = id };
        }
        else if (id != command.Id)
        {
            return BadRequest("菜单组 ID 不匹配");
        }

        var result = await _mediator.Send(command);
        return result.Success ? Success() : Fail(result.Message);
    }

    /// <summary>
    /// 删除菜单组
    /// </summary>
    [HttpDelete("groups/{id:guid}")]
    public async Task<IActionResult> DeleteMenuGroup(Guid id)
    {
        var command = new DeleteMenuGroupCommand(id);
        var result = await _mediator.Send(command);
        return result.Success ? Success() : Fail(result.Message);
    }

    #endregion

    #region 菜单排序

    /// <summary>
    /// 更新菜单排序
    /// </summary>
    [HttpPut("sort")]
    public async Task<IActionResult> UpdateMenuSort([FromBody] UpdateMenuSortCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Success ? Success() : Fail(result.Message);
    }

    #endregion

    #region 批量操作

    /// <summary>
    /// 批量删除菜单
    /// </summary>
    [HttpDelete("batch")]
    public async Task<IActionResult> BatchDeleteMenus([FromBody] BatchDeleteMenusCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Success ? Success() : Fail(result.Message);
    }

    /// <summary>
    /// 批量更新菜单状态
    /// </summary>
    [HttpPut("batch/status")]
    public async Task<IActionResult> BatchUpdateMenuStatus([FromBody] BatchUpdateMenuStatusCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Success ? Success() : Fail(result.Message);
    }

    #endregion
}