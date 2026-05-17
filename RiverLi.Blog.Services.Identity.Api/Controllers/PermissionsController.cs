using Microsoft.AspNetCore.Mvc;
using MediatR;
using RiverLi.Blog.Identity.Application.Permissions.Commands;
using RiverLi.Blog.Identity.Application.Permissions.Queries;
//using RiverLi.Blog.Infrastructure.Shared.Controllers;

namespace RiverLi.Blog.Identity.Api.Controllers;

/// <summary>
/// 权限资源控制器
/// 合并原 PermissionController + PermissionManagementController
/// 职责：权限 CRUD、分页、分组查询
/// 权限分配由 PermissionAssignmentsController 负责
/// </summary>
[ApiController]
[Route("api/permissions")]  // 添加 api 前缀
// 移除 api 前缀，直接使用 /permissions
public class PermissionsController : BaseApiController
{
    private readonly IMediator _mediator;

    public PermissionsController(IMediator mediator)
        => _mediator = mediator;

    // ─── Queries ─────────────────────────────────

    /// <summary>获取权限列表（支持过滤）</summary>
    [HttpGet]
    public async Task<IActionResult> GetPermissions(
        [FromQuery] string? group = null,
        [FromQuery] bool? enabled = null,
        [FromQuery] string? keyword = null)
    {
        var query = new GetAllPermissionsQuery(group, enabled, keyword);
        var result = await _mediator.Send(query);
        return result.Success ? Success(result.Data) : Fail(result.Message);
    }

    /// <summary>分页获取权限列表</summary>
    [HttpGet("paged")]
    public async Task<IActionResult> GetPagedPermissions(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? group = null,
        [FromQuery] bool? enabled = null,
        [FromQuery] string? keyword = null,
        [FromQuery] string? sortBy = "Name",
        [FromQuery] bool sortDesc = false)
    {
        var query = new GetPagedPermissionsQuery(pageIndex, pageSize, group, enabled, keyword, sortBy, sortDesc);
        var result = await _mediator.Send(query);
        return result.Success ? Success(result.Data,result.TotalCount,result.PageIndex,result.PageSize) : Fail(result.Message);
    }

    /// <summary>按分组获取权限</summary>
    [HttpGet("grouped")]
    public async Task<IActionResult> GetGroupedPermissions([FromQuery] string? searchKeyword = null)
    {
        var query = new GetGroupedPermissionsQuery(searchKeyword);
        var result = await _mediator.Send(query);
        return result.Success ? Success(result.Data) : Fail(result.Message);
    }

    /// <summary>获取权限统计信息</summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        var query = new GetPermissionStatisticsQuery();
        var result = await _mediator.Send(query);
        return result.Success ? Success(result.Data) : Fail(result.Message);
    }

    /// <summary>根据 ID 获取权限详情</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPermission(Guid id)
    {
        var query = new GetPermissionByIdQuery(id);
        var result = await _mediator.Send(query);
        return result.Success ? Success(result.Data) : NotFound(result.Message);
    }

    /// <summary>根据编码获取权限</summary>
    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetPermissionByCode(string code)
    {
        var query = new GetPermissionByCodeQuery(code);
        var result = await _mediator.Send(query);
        return result.Success ? Success(result.Data) : NotFound(result.Message);
    }

    /// <summary>获取用户拥有的权限列表</summary>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserPermissions(Guid userId)
    {
        var query = new GetUserPermissionsQuery(userId);
        var result = await _mediator.Send(query);
        return result.Success ? Success(result.Data) : NotFound(result.Message);
    }

    /// <summary>获取角色拥有的权限列表</summary>
    [HttpGet("role/{roleId}")]
    public async Task<IActionResult> GetRolePermissions(Guid roleId)
    {
        var query = new GetRolePermissionsQuery(roleId);
        var result = await _mediator.Send(query);
        return result.Success ? Success(result.Data) : NotFound(result.Message);
    }

    /// <summary>检查用户是否拥有特定权限</summary>
    [HttpGet("user/{userId}/check/{permissionCode}")]
    public async Task<IActionResult> CheckUserPermission(Guid userId, string permissionCode)
    {
        var query = new CheckUserPermissionQuery(userId, permissionCode);
        var result = await _mediator.Send(query);
        return Success(new { hasPermission = result.Data, permissionCode });
    }

    /// <summary>获取角色的权限分配状态</summary>
    [HttpGet("roles/{roleId}/permission-status")]
    public async Task<IActionResult> GetRolePermissionStatus(Guid roleId)
    {
        var query = new GetRolePermissionStatusQuery(roleId);
        var result = await _mediator.Send(query);
        return result.Success ? Success(result.Data) : Fail(result.Message);
    }

    // ─── Commands ────────────────────────────────

    /// <summary>创建权限</summary>
    [HttpPost]
    public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Success ? Success(result.Data) : Fail(result.Message);
    }

    /// <summary>更新权限</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdatePermission(Guid id, [FromBody] UpdatePermissionCommand command)
    {
        var cmd = command with { Id = id };
        var result = await _mediator.Send(cmd);
        return result.Success ? Success() : Fail(result.Message);
    }

    /// <summary>删除权限</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePermission(Guid id)
    {
        var result = await _mediator.Send(new DeletePermissionCommand(id));
        return result.Success ? Success() : Fail(result.Message);
    }

    /// <summary>批量删除权限</summary>
    [HttpDelete("batch")]
    public async Task<IActionResult> BatchDeletePermissions([FromBody] List<Guid> ids)
    {
        var result = await _mediator.Send(new BatchDeletePermissionsCommand(ids));
        return result.Success ? Success() : Fail(result.Message);
    }

    /// <summary>批量为角色分配权限</summary>
    [HttpPost("roles/{roleId}/permissions/batch")]
    public async Task<IActionResult> BatchAssignPermissions(Guid roleId, [FromBody] BatchAssignPermissionsRequest request)
    {
        var command = new BatchAssignPermissionsCommand(roleId, request.AddPermissionIds, request.RemovePermissionIds);
        var result = await _mediator.Send(command);
        return result.Success ? Success(result.Message ?? "权限分配成功") : Fail(result.Message);
    }
}

/// <summary>
/// 批量分配权限请求
/// </summary>
public class BatchAssignPermissionsRequest
{
    /// <summary>
    /// 要添加的权限 ID 列表
    /// </summary>
    public List<Guid> AddPermissionIds { get; set; } = new();
    
    /// <summary>
    /// 要移除的权限 ID 列表
    /// </summary>
    public List<Guid> RemovePermissionIds { get; set; } = new();
}
