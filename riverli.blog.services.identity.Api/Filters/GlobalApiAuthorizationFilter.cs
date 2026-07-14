using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RiverLi.Blog.Infrastructure.Shared.Security;
using riverli.blog.services.identity.Application.Interfaces;

namespace riverli.blog.services.identity.Api.Filters;

/// <summary>
/// 全局 API 授权过滤器 — 自动拦截所有请求，比对 Method+Route 与 ApiResource 表
/// 替代手动的 [RequirePermission] 标签
/// </summary>
public class GlobalApiAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly IApiPermissionChecker _permissionChecker;

    public GlobalApiAuthorizationFilter(IApiPermissionChecker permissionChecker)
    {
        _permissionChecker = permissionChecker;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // 1. 未登录 → 交给框架的 [Authorize] 或认证中间件处理，不在此拦截
        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
            return;

        // 2. 超级管理员 → 直接放行
        if (context.HttpContext.User.IsInRole("Admin"))
            return;

        // 3. 从路由元数据中获取路由模板（如 /api/identity/users/{id}）
        var endpoint = context.HttpContext.GetEndpoint();
        if (endpoint is not RouteEndpoint routeEndpoint)
            return; // 非路由端点（如 SignalR），放行

        var routePattern = routeEndpoint.RoutePattern.RawText;
        if (string.IsNullOrEmpty(routePattern))
            return;

        var httpMethod = context.HttpContext.Request.Method;

        // 4. 获取用户 ID
        var userIdClaim = context.HttpContext.User
            .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return; // 无法解析用户 ID，放行（由认证层兜底）

        // 5. 调用权限检查器
        var isGranted = await _permissionChecker.HasApiPermissionAsync(userId, routePattern, httpMethod);

        if (!isGranted)
        {
            context.Result = new ObjectResult(new
            {
                code = 403,
                message = "禁止访问：您没有调用该接口的权限",
                success = false
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}
