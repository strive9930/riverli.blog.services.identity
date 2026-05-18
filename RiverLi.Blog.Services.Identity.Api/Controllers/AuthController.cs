using Microsoft.AspNetCore.Mvc;
using MediatR;
using RiverLi.Blog.Identity.Application.Auth.Commands.Register;
using RiverLi.Blog.Identity.Application.Auth.Commands.Login;
using RiverLi.Blog.Identity.Application.Auth.Queries.GetCurrentUserInfo;
using RiverLi.Blog.Infrastructure.Shared.Controllers;

namespace RiverLi.Blog.Identity.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : BaseApiController
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
            => _mediator = mediator;

        #region 基础身份认证 (Login/Register)

        /// <summary>用户注册</summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Success ? Success(result.Data) : Fail(result.Message);
        }

        /// <summary>用户登录</summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Success ? Success(result.Data) : Unauthorized(result.Message);
        }

        #endregion

        /// <summary>获取当前登录用户信息</summary>
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            Console.WriteLine($"AuthController.GetCurrentUser - User.Identity.IsAuthenticated: {User.Identity?.IsAuthenticated}");
            Console.WriteLine($"AuthController.GetCurrentUser - User.Identity.Name: {User.Identity?.Name}");
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            Console.WriteLine($"AuthController.GetCurrentUser - NameIdentifier claim: {userIdClaim?.Value}");
            var subClaim = User.FindFirst("sub");
            Console.WriteLine($"AuthController.GetCurrentUser - sub claim: {subClaim?.Value}");
            
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized("无法识别用户身份");
            }

            var query = new GetCurrentUserInfoQuery(userId);
            var result = await _mediator.Send(query);
            return result.Success ? Success(result.Data) : Fail(result.Message);
        }
    }
}
