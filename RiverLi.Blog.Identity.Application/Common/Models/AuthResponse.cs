using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiverLi.Blog.Identity.Application.Common.Models
{
    /// <summary>
    /// 身份认证成功后的返回信息
    /// </summary>
    /// <param name="Token">JWT 访问令牌</param>
    /// <param name="Expiration">过期时间</param>
    /// <param name="UserId">用户ID</param>
    public record AuthResponse(
        string Token,
        DateTime Expiration,
        Guid UserId,
        string? Nickname);
}
