using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiverLi.Blog.Identity.Application.Users.Commands.User;
using RiverLi.Blog.Infrastructure.Shared.Controllers;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Api.Controllers;

/// <summary>
/// 用户前端数据控制器
/// 提供当前用户的信息、菜单权限等前端所需数据
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserFrontendController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserFrontendController(
        IMediator mediator,
        IHttpContextAccessor httpContextAccessor)
    {
        _mediator = mediator;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 获取当前用户基本信息
    /// </summary>
    [HttpGet("info")]
    public async Task<ActionResult<Result<UserInfoDto>>> GetCurrentUserInfo()
    {
        try
        {
            var userId = UserId;
            if (userId == null)
            {
                return Unauthorized(new { message = "未登录" });
            }

            var query = new GetUserInfoQuery(userId.Value);
            var result = await _mediator.Send(query);
            
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "获取用户信息失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 获取当前用户的菜单权限（树形结构）
    /// </summary>
    [HttpGet("menus")]
    public async Task<ActionResult<Result<List<MenuTreeDto>>>> GetUserMenus()
    {
        try
        {
            var userId = UserId;
            if (userId == null)
            {
                return Unauthorized(new { message = "未登录" });
            }

            var query = new GetUserMenusQuery(userId.Value);
            var result = await _mediator.Send(query);
            
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "获取菜单失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 获取当前用户的权限码列表
    /// </summary>
    [HttpGet("permissions")]
    public async Task<ActionResult<Result<List<string>>>> GetUserPermissions()
    {
        try
        {
            var userId = UserId;
            if (userId == null)
            {
                return Unauthorized(new { message = "未登录" });
            }

            var query = new GetUserPermissionCodesQuery(userId.Value);
            var result = await _mediator.Send(query);
            
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "获取权限失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 获取当前用户的角色列表
    /// </summary>
    [HttpGet("roles")]
    public async Task<ActionResult<Result<List<RoleDto>>>> GetUserRoles()
    {
        try
        {
            var userId = UserId;
            if (userId == null)
            {
                return Unauthorized(new { message = "未登录" });
            }

            var query = new GetCurrentUserRolesQuery(userId.Value);
            var result = await _mediator.Send(query);
            
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "获取角色失败", error = ex.Message });
        }
    }

    /// <summary>
    /// 获取仪表盘统计数据
    /// </summary>
    [HttpGet("dashboard/stats")]
    public async Task<ActionResult<Result<DashboardStatsDto>>> GetDashboardStats()
    {
        try
        {
            var userId = UserId;
            if (userId == null)
            {
                return Unauthorized(new { message = "未登录" });
            }

            var query = new GetDashboardStatsQuery(userId.Value);
            var result = await _mediator.Send(query);
            
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "获取统计数据失败", error = ex.Message });
        }
    }

    #region Helper Methods

    /// <summary>
    /// 获取当前登录用户的 ID
    /// </summary>
    private Guid? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("sub");
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        return null;
    }

    #endregion
}
