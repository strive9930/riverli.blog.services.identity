using Microsoft.AspNetCore.Mvc;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Api.Controllers;

/// <summary>
/// API 控制器基类
/// 提供统一的响应格式和辅助方法
/// </summary>
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// 返回成功响应（无数据）
    /// </summary>
    protected IActionResult Success()
    {
        return Ok(new Result<object>
        {
            Success = true,
            Message = "操作成功"
        });
    }

    /// <summary>
    /// 返回成功响应（带数据）
    /// </summary>
    protected IActionResult Success(object data)
    {
        return Ok(new Result<object>
        {
            Success = true,
            Data = data,
            Message = "操作成功"
        });
    }

    /// <summary>
    /// 返回成功响应（带数据和消息）
    /// </summary>
    protected IActionResult Success(object data, string message)
    {
        return Ok(new Result<object>
        {
            Success = true,
            Data = data,
            Message = message
        });
    }

    /// <summary>
    /// 返回分页成功响应
    /// </summary>
    protected IActionResult Success(object data, int totalCount, int pageIndex, int pageSize)
    {
        return Ok(new
        {
            Success = true,
            Data = data,
            TotalCount = totalCount,
            PageIndex = pageIndex,
            PageSize = pageSize,
            Message = "操作成功"
        });
    }

    /// <summary>
    /// 返回失败响应
    /// </summary>
    protected IActionResult Fail(string message)
    {
        return BadRequest(new Result<object>
        {
            Success = false,
            Message = message
        });
    }

    /// <summary>
    /// 返回失败响应（带错误码）
    /// </summary>
    protected IActionResult Fail(string message, int statusCode)
    {
        return StatusCode(statusCode, new Result<object>
        {
            Success = false,
            Message = message
        });
    }

    /// <summary>
    /// 获取当前用户ID
    /// </summary>
    protected Guid? UserId
    {
        get
        {
            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("UserId");
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
            return null;
        }
    }

    /// <summary>
    /// 获取当前用户邮箱
    /// </summary>
    protected string? UserEmail
    {
        get => User.FindFirst("email")?.Value;
    }

    /// <summary>
    /// 获取当前用户名
    /// </summary>
    protected string? UserName
    {
        get => User.FindFirst("name")?.Value ?? User.Identity?.Name;
    }
}
