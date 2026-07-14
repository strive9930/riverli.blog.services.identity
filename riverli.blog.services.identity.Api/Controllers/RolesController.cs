using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiverLi.Blog.Infrastructure.Shared.Controllers;
using riverli.blog.services.identity.Application.DTOs.Roles;
using riverli.blog.services.identity.Application.Features.Roles.Commands;
using riverli.blog.services.identity.Application.Features.Roles.Queries;

namespace riverli.blog.services.identity.Api.Controllers;

[ApiController]
[Route("api/identity/[controller]")]
public class RolesController : BaseApiController
{
    private readonly IMediator _mediator;

    public RolesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Description("获取所有角色列表")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var result = await _mediator.Send(new GetRolesQuery());
            return Success(result);
        }
        catch (Exception ex)
        {
            return Fail($"获取角色列表失败: {ex.Message}");
        }
    }

    [HttpGet("page")]
    [Description("分页查询角色")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPage([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, [FromQuery] string? keyword = null)
    {
        try
        {
            var result = await _mediator.Send(new GetRolesPageQuery(pageIndex, pageSize, keyword));
            return result.Success ? Success(result.Data, result.TotalCount, result.PageIndex, result.PageSize) : Fail(result.Message);
        }
        catch (Exception ex)
        {
            return Fail($"分页查询角色失败: {ex.Message}");
        }
    }

    [HttpPost]
    [Description("创建角色")]
    public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
    {
        try
        {
            var result = await _mediator.Send(new CreateRoleCommand(dto));
            return result ? Success("创建成功") : Fail("创建角色失败");
        }
        catch (Exception ex)
        {
            return Fail($"创建角色失败: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    [Description("更新角色")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleDto dto)
    {
        try
        {
            var result = await _mediator.Send(new UpdateRoleCommand(id, dto));
            return result ? Success("更新成功") : Fail("更新角色失败");
        }
        catch (Exception ex)
        {
            return Fail($"更新角色失败: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    [Description("删除角色")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new DeleteRoleCommand(id));
            return result ? Success("删除成功") : Fail("删除角色失败");
        }
        catch (Exception ex)
        {
            return Fail($"删除角色失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取指定角色的菜单权限 ID 集合
    /// </summary>
    [HttpGet("{id:guid}/menus")]
    [Description("获取角色菜单权限")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRoleMenus(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetRoleMenusQuery(id));
            return Success(result);
        }
        catch (Exception ex)
        {
            return Fail($"获取角色菜单权限失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 为指定角色分配菜单权限
    /// </summary>
    [HttpPost("{id:guid}/menus")]
    [Description("分配角色菜单权限")]
    public async Task<IActionResult> AssignMenus(Guid id, [FromBody] AssignRoleMenusRequest request)
    {
        try
        {
            var result = await _mediator.Send(new UpdateRoleMenusCommand(id, request.MenuIds));
            return result ? Success("权限分配成功") : Fail("权限分配失败");
        }
        catch (Exception ex)
        {
            return Fail($"分配菜单权限失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取指定角色绑定的所有 API 接口 ID
    /// </summary>
    [HttpGet("{id:guid}/apis")]
    [Description("获取角色API权限")]
    public async Task<IActionResult> GetRoleApis(Guid id)
    {
        try
        {
            var query = new GetRoleApisQuery(id);
            var result = await _mediator.Send(query);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Fail($"获取角色API权限失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 为指定角色全量分配 API 接口权限
    /// </summary>
    [HttpPost("{id:guid}/apis")]
    [Description("分配角色API权限")]
    public async Task<IActionResult> AssignRoleApis(Guid id, [FromBody] AssignRoleApisRequest request)
    {
        try
        {
            var result = await _mediator.Send(new UpdateRoleApisCommand(id, request.ApiIds));
            return result ? Success("权限分配成功") : Fail("权限分配失败");
        }
        catch (Exception ex)
        {
            return Fail($"分配API权限失败: {ex.Message}");
        }
    }
}
