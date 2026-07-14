// Api/Controllers/UsersController.cs
using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiverLi.Blog.Infrastructure.Shared.Controllers;
using riverli.blog.services.identity.Application.DTOs.Users;
using riverli.blog.services.identity.Application.Features.Users;
using riverli.blog.services.identity.Application.Features.Users.Commands;
using riverli.blog.services.identity.Application.Features.Users.Queries;

namespace riverli.blog.services.identity.Api.Controllers;

[ApiController]
[Route("api/identity/[controller]")]
public class UsersController : BaseApiController
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("page")]
    [Description("分页查询用户")]
    public async Task<IActionResult> GetPage([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, [FromQuery] string? keyword = null)
    {
        try
        {
            var result = await _mediator.Send(new GetUsersPageQuery(pageIndex, pageSize, keyword));
            return result.Success ? Success(result.Data, result.TotalCount, result.PageIndex, result.PageSize) : Fail(result.Message);
        }
        catch (Exception ex)
        {
            return Fail($"分页查询用户失败: {ex.Message}");
        }
    }

    [HttpPost]
    [Description("创建用户")]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        try
        {
            var result = await _mediator.Send(new CreateUserCommand(dto));
            return result ? Success("创建成功") : Fail("创建用户失败");
        }
        catch (Exception ex)
        {
            return Fail($"创建用户失败: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    [Description("更新用户")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
    {
        try
        {
            var result = await _mediator.Send(new UpdateUserCommand(id, dto));
            return result ? Success("更新成功") : Fail("更新用户失败");
        }
        catch (Exception ex)
        {
            return Fail($"更新用户失败: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    [Description("删除用户")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new DeleteUserCommand(id));
            return result ? Success("删除成功") : Fail("删除用户失败");
        }
        catch (KeyNotFoundException ex)
        {
            return Fail(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return Fail($"删除用户失败: {ex.Message}");
        }
    }

    [HttpPost("{id}/roles")]
    [Description("分配用户角色")]
    public async Task<IActionResult> AssignRoles(Guid id, [FromBody] AssignUserRolesRequest request)
    {
        try
        {
            var result = await _mediator.Send(new AssignRolesToUserCommand(id, request.RoleIds));
            return result ? Success("分配角色成功") : Fail("分配角色失败");
        }
        catch (Exception ex)
        {
            return Fail($"分配角色失败: {ex.Message}");
        }
    }
    /// <summary>
    /// 获取指定用户绑定的所有角色 ID (用于前端回显)
    /// </summary>
    [HttpGet("{id:guid}/roles")]
    [Description("查询用户已绑定的角色")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUserRoles(Guid id)
    {
        try
        {
            var query = new GetUserRolesQuery(id);
            var result = await _mediator.Send(query);
            return Success(result);
        }
        catch (Exception ex)
        {
            return Fail($"查询用户角色失败: {ex.Message}");
        }
    }
}