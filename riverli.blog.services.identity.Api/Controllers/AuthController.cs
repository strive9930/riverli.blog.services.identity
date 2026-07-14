// Api/Controllers/AuthController.cs

using System.Security.Claims;
using System.ComponentModel;
using System.Security.Authentication;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiverLi.Blog.Infrastructure.Shared.Controllers;
using riverli.blog.services.identity.Application.Features.Menus.Queries;
using riverli.blog.services.identity.Application.Features.Users.Queries; // 根据您的实际命名空间调整

namespace riverli.blog.services.identity.Api.Controllers;

[ApiController]
[Route("api/identity/[controller]")]
public class AuthController : BaseApiController
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    [Description("用户登录")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginQuery query)
    {
        try
        {
            var result = await _mediator.Send(query);
            return result.Success ? Success(result.Data) : Fail(result.Message);
        }
        catch (InvalidCredentialException ex)
        {
            return Fail(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return Fail($"登录失败: {ex.Message}");
        }
    }

    // 测试需要鉴权的接口
    [HttpGet("test-auth")]
    [Description("测试认证接口")]
    [Authorize] // 只有携带有效 Token 才能访问
    public IActionResult TestAuth()
    {
        // 获取当前登录用户的 ID 和名称
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userName = User.Identity?.Name;
        
        return Ok(new { message = $"验证成功！你好, {userName} (ID: {userId})" });
    }
    /// <summary>
    /// 获取当前登录用户的专属动态菜单树
    /// </summary>
    [HttpGet("my-menus")]
    [Description("获取当前用户菜单树")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMyMenus()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Fail("无法识别当前用户身份");

            var result = await _mediator.Send(new GetUserMenusQuery(userId));
            return Success(result);
        }
        catch (Exception ex)
        {
            return Fail($"获取用户菜单失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 获取当前登录用户的后端 API 接口权限标识列表
    /// 返回格式: ["GET:/api/users", "POST:/api/users", ...]
    /// </summary>
    [HttpGet("my-permissions")]
    [Description("获取当前用户API权限")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMyPermissions()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Fail("无法识别当前用户身份");

            var result = await _mediator.Send(new GetMyPermissionsQuery(userId));
            return Success(result);
        }
        catch (Exception ex)
        {
            return Fail($"获取用户权限失败: {ex.Message}");
        }
    }
}