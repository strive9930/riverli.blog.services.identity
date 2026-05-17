using Microsoft.AspNetCore.Mvc;
using MediatR;
using RiverLi.Blog.Identity.Application.Users.Commands;
using RiverLi.Blog.Identity.Application.Users.Commands.User;
using RiverLi.Blog.Identity.Application.Users.Queries.GetAllUsers;
using RiverLi.Blog.Identity.Application.Users.Queries.GetUserById;
using RiverLi.Blog.Identity.Application.Users.Queries.GetUserRoles;
using RiverLi.Blog.Identity.Application.Permissions.Commands;
//using RiverLi.Blog.Infrastructure.Shared.Controllers;

namespace RiverLi.Blog.Identity.Api.Controllers;

/// <summary>
/// 用户资源控制器
/// </summary>
[ApiController]
[Route("api/users")]
public class UsersController : BaseApiController
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
        => _mediator = mediator;

    /// <summary>获取用户列表（不分页，返回所有用户）</summary>
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var query = new RiverLi.Blog.Identity.Application.Users.Queries.GetAllUsers.GetAllUsersQuery(null, null, 1, int.MaxValue);
        var result = await _mediator.Send(query);
        return result.Success ? Success(result.Data, result.TotalCount, result.PageIndex, result.PageSize) : Fail(result.Message);
    }

    /// <summary>获取用户列表</summary>
    [HttpGet("paged")]
    public async Task<IActionResult> GetPageUsers(
        [FromQuery] string? keyword = null,
        [FromQuery] bool? isEnabled = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new RiverLi.Blog.Identity.Application.Users.Queries.GetAllUsers.GetAllUsersQuery(keyword, isEnabled, page, pageSize);
        var result = await _mediator.Send(query);
        return result.Success ? Success(result.Data,result.TotalCount,result.PageIndex,result.PageSize) : Fail(result.Message);
    }

    /// <summary>获取用户详情</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var query = new RiverLi.Blog.Identity.Application.Users.Queries.GetUserById.GetUserByIdQuery(id);
        var result = await _mediator.Send(query);
        
        if (!result.Success)
        {
            return NotFound(new { message = "用户不存在", error = result.Message });
        }
        
        var userDto = new
        {
            id = result?.Data?.Id.ToString(),
            username = result?.Data?.UserName,
            email = result?.Data?.Email,
            nickname = result?.Data?.NickName,
            phoneNumber = result?.Data?.PhoneNumber,
            isEnabled = result?.Data?.IsEnabled, 
            createTime = result?.Data?.CreateTime,
            updateTime = result?.Data?.UpdateTime,
            lastLoginTime = (DateTime?)null // ApplicationUser 中没有这个字段
        };
        
        return Success(userDto);
    }

    /// <summary>创建用户</summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var command = new CreateUserCommand(
            request.Email,
            request.Password,
            request.NickName,
            request.PhoneNumber,
            request.IsEnabled
        );
        
        var result = await _mediator.Send(command);
        
        return result.Success ? Success(new { id = result.Data.ToString() }) : Fail(result.Message);
    }

    /// <summary>更新用户</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        var command = new UpdateUserCommand(
            id,
            request.Email,
            request.NickName,
            request.PhoneNumber,
            request.IsEnabled
        );
        
        var result = await _mediator.Send(command);
        
        return result.Success ? Success() : Fail(result.Message);
    }

    /// <summary>删除用户</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var command = new DeleteUserCommand(id);
        var result = await _mediator.Send(command);
        
        return result.Success ? Success() : Fail(result.Message);
    }

    /// <summary>获取用户角色</summary>
    [HttpGet("{userId:guid}/roles")]
    public async Task<IActionResult> GetUserRoles(Guid userId)
    {
        var query = new RiverLi.Blog.Identity.Application.Users.Queries.GetUserRoles.GetUserRolesQuery(userId);
        var result = await _mediator.Send(query);
        
        return result.Success ? Success(result.Data) : Fail(result.Message);
    }

    /// <summary>为用户分配角色</summary>
    [HttpPost("{userId:guid}/roles")]
    public async Task<IActionResult> AssignUserRoles(Guid userId, [FromBody] AssignUserRolesRequest request)
    {
        var command = new AssignUserRolesCommand(userId, request.RoleIds);
        var result = await _mediator.Send(command);
        
        return result.Success ? Success("角色分配成功") : Fail(result.Message);
    }

    /// <summary>获取用户权限</summary>
    [HttpGet("{userId:guid}/permissions")]
    public async Task<IActionResult> GetUserPermissions(Guid userId)
    {
        var query = new RiverLi.Blog.Identity.Application.Permissions.Commands.GetUserPermissionsQuery(userId);
        var result = await _mediator.Send(query);
        
        return result.Success ? Success(result.Data) : Fail(result.Message);
    }

    /// <summary>更新用户密码</summary>
    [HttpPut("{id:guid}/password")]
    public async Task<IActionResult> UpdateUserPassword(Guid id, [FromBody] ChangePasswordRequest request)
    {
        var command = new ChangePasswordCommand(
            UserId: id,
            CurrentPassword: request.CurrentPassword,
            NewPassword: request.NewPassword
        );

        var result = await _mediator.Send(command);

        return result.Success ? Success("密码更新成功") : Fail(result.Message);
    }
}

/// <summary>
/// 创建用户请求 DTO
/// </summary>
public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? NickName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// 更新用户请求 DTO
/// </summary>
public class UpdateUserRequest
{
    public string? Email { get; set; }
    public string? NickName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? IsEnabled { get; set; }
}

/// <summary>
/// 分配用户角色请求 DTO
/// </summary>
public class AssignUserRolesRequest
{
    public List<Guid> RoleIds { get; set; } = new();
}

/// <summary>
/// 修改密码请求 DTO
/// </summary>
public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
