// Api/Controllers/MenusController.cs
using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiverLi.Blog.Infrastructure.Shared.Controllers;
using riverli.blog.services.identity.Application.DTOs;
using riverli.blog.services.identity.Application.Features.Menus.Commands;
using riverli.blog.services.identity.Application.Features.Menus.Queries;

namespace riverli.blog.services.identity.Api.Controllers;

[ApiController]
[Route("api/identity/[controller]")]
public class MenusController : BaseApiController
{
    private readonly IMediator _mediator;

    public MenusController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("tree")]
    [Description("获取菜单树")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTree()
    {
        try
        {
            var result = await _mediator.Send(new GetMenuTreeQuery());
            return Success(result);
        }
        catch (Exception ex)
        {
            return Fail($"获取菜单树失败: {ex.Message}");
        }
    }

    [HttpPost]
    [Description("创建菜单")]
    public async Task<IActionResult> Create([FromBody] CreateOrUpdateMenuDto dto)
    {
        try
        {
            var result = await _mediator.Send(new CreateMenuCommand(dto));
            return result ? Success("创建成功") : Fail("创建菜单失败");
        }
        catch (Exception ex)
        {
            return Fail($"创建菜单失败: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    [Description("更新菜单")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateOrUpdateMenuDto dto)
    {
        try
        {
            var result = await _mediator.Send(new UpdateMenuCommand(id, dto));
            return result ? Success("更新成功") : Fail("更新菜单失败");
        }
        catch (Exception ex)
        {
            return Fail($"更新菜单失败: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    [Description("删除菜单")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new DeleteMenuCommand(id));
            return result ? Success("删除成功") : Fail("删除菜单失败");
        }
        catch (Exception ex)
        {
            return Fail($"删除菜单失败: {ex.Message}");
        }
    }
}