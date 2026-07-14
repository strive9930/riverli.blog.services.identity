// Domain/Entities/AppUser.cs
using Microsoft.AspNetCore.Identity;
using RiverLi.DDD.Core.Domain.Common;

namespace riverli.blog.services.identity.Domain.Entities;

/// <summary>
/// 应用用户实体，扩展自 ASP.NET Core Identity 的 IdentityUser&lt;Guid&gt;
/// </summary>
public class AppUser : IdentityUser<Guid>, IAggregateRoot, IEntity<Guid>
{
    /// <summary>
    /// 真实姓名
    /// </summary>
    public string RealName { get; set; } = string.Empty;

    /// <summary>
    /// 头像地址
    /// </summary>
    public string Avatar { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用（true=启用，false=禁用）
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 创建时间（UTC）
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

