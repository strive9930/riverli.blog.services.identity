using MediatR;
using System.ComponentModel.DataAnnotations;
using riverli.blog.services.identity.Application.DTOs.Auth;
using RiverLi.DDD.Core.Application.Common.Models;

namespace riverli.blog.services.identity.Application.Features.Users.Queries;

/// <summary>
/// 用户登录查询请求
/// 继承 IRequest<LoginResponseDto> 告诉 MediatR：处理这个请求后，必须返回一个 LoginResponseDto
/// </summary>
public record LoginQuery : IRequest<Result<LoginResponseDto>>
{
    /// <summary>
    /// 用户名
    /// </summary>
    [Required(ErrorMessage = "用户名不能为空")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "用户名长度必须在 3 到 50 个字符之间")]
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// 密码
    /// </summary>
    [Required(ErrorMessage = "密码不能为空")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "密码长度不能少于 6 个字符")]
    public string Password { get; init; } = string.Empty;
}