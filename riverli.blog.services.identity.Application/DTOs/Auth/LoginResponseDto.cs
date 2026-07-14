namespace riverli.blog.services.identity.Application.DTOs.Auth;

/// <summary>
/// 登录成功后的响应数据传输对象 (DTO)
/// </summary>
/// <param name="Token">JWT 鉴权令牌，前端需将其存入 localStorage 或 Cookie</param>
/// <param name="Username">用户登录账号</param>
/// <param name="RealName">用户真实姓名，用于前端右上角展示</param>
/// <param name="Roles">用户所属的角色集合，前端可据此做粗粒度的按钮隐藏</param>
public record LoginResponseDto(
    string Token,
    string Username,
    string RealName,
    IEnumerable<string> Roles
);