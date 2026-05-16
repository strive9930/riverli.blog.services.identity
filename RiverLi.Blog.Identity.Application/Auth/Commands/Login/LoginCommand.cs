using MediatR;
using Microsoft.AspNetCore.Identity;
using RiverLi.DDD.Core.Application.Common.Models;
using RiverLi.Blog.Identity.Application.Common.Models;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Auth; //JwtTokenService

namespace RiverLi.Blog.Identity.Application.Auth.Commands.Login
{
    /// <summary>
    /// 登录请求 DTO
    /// </summary>
    public record LoginCommand(
        string Email,
        string Password) : IRequest<Result<AuthResponse>>;
   
}