using MediatR;
using Microsoft.AspNetCore.Mvc;
using RiverLi.Blog.Identity.Application.Users.Commands.User;
using RiverLi.DDD.Core.Application.Common.Models;
//using RiverLi.Blog.Infrastructure.Shared.Controllers;

namespace RiverLi.Blog.Identity.Api.Controllers
{
    /// <summary>
    /// 用户登录历史资源控制器
    /// </summary>
    [ApiController]
    [Route("api/user-login-history")]
    public class UserLoginHistoryController : BaseApiController
    {
        private readonly IMediator _mediator;

        public UserLoginHistoryController(IMediator mediator)
            => _mediator = mediator;

        /// <summary>获取用户登录历史（支持分页和筛选）</summary>
        [HttpGet]
        public async Task<IActionResult> GetUserLoginHistory(
            [FromQuery] Guid? userId = null,
            [FromQuery] string? ipAddress = null,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? startTime = null,
            [FromQuery] DateTime? endTime = null,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetUserLoginHistoryQuery(
                userId, ipAddress, status, startTime, endTime, pageIndex, pageSize);
            
            var result = await _mediator.Send(query);
            return Success(result);
        }

        /// <summary>获取指定用户的最近登录历史</summary>
        [HttpGet("{userId:guid}/recent")]
        public async Task<IActionResult> GetUserRecentLoginHistory(
            Guid userId,
            [FromQuery] int count = 10)
        {
            var query = new GetUserRecentLoginHistoryQuery(userId, count);
            var result = await _mediator.Send(query);
            return result.Success ? Success(result.Data) : Fail(result.Message);
        }

        /// <summary>获取用户登录统计信息</summary>
        [HttpGet("{userId:guid}/statistics")]
        public async Task<IActionResult> GetUserLoginStatistics(Guid userId)
        {
            var query = new GetUserLoginStatisticsQuery(userId);
            var result = await _mediator.Send(query);
            return result.Success ? Success(result.Data) : Fail(result.Message);
        }

        /// <summary>获取当前用户的登录历史</summary>
        [HttpGet("my-history")]
        public async Task<IActionResult> GetMyLoginHistory(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            // 使用 NameIdentifier claim 而不是 sub
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userGuid))
            {
                return Unauthorized("无法获取当前用户信息");
            }

            var query = new GetUserLoginHistoryQuery(
                userGuid, null, null, null, null, pageIndex, pageSize);
            
            var result = await _mediator.Send(query);
            return Success(result);
        }

        /// <summary>获取当前用户的登录统计信息</summary>
        [HttpGet("my-statistics")]
        public async Task<IActionResult> GetMyLoginStatistics()
        {
            // 使用 NameIdentifier claim 而不是 sub
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userGuid))
            {
                return Unauthorized("无法获取当前用户信息");
            }

            var query = new GetUserLoginStatisticsQuery(userGuid);
            var result = await _mediator.Send(query);
            return result.Success ? Success(result.Data) : Fail(result.Message);
        }
    }
}
