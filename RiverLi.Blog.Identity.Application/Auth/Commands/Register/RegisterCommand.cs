using MediatR;
using Microsoft.AspNetCore.Identity;
using RiverLi.DDD.Core.Application.Common.Models; // 假设 Result 类在�?
using RiverLi.DDD.Core.Domain.Events;
using RiverLi.Blog.Identity.Domain.Entities;

namespace RiverLi.Blog.Identity.Application.Auth.Commands.Register
{
    /// <summary>
    /// 注册请求 DTO
    /// </summary>
    public record RegisterCommand(
        string Email,
        string Password,
        string NickName) : IRequest<Result<Guid>>;

    
}