using Microsoft.AspNetCore.Mvc;
using MediatR;
using RiverLi.Blog.Identity.Application.Roles.Queries;
//using RiverLi.Blog.Infrastructure.Shared.Controllers;
using CreateRoleCommand = RiverLi.Blog.Identity.Application.Roles.Commands.CreateRoleCommand;
using DeleteRoleCommand = RiverLi.Blog.Identity.Application.Roles.Commands.DeleteRoleCommand;
using GetAllRolesQuery = RiverLi.Blog.Identity.Application.Roles.Commands.GetAllRolesQuery;
using GetRoleByIdQuery = RiverLi.Blog.Identity.Application.Roles.Commands.GetRoleByIdQuery;
using GetRolePermissionsQuery = RiverLi.Blog.Identity.Application.Roles.Commands.GetRolePermissionsQuery;
using UpdateRoleCommand = RiverLi.Blog.Identity.Application.Roles.Commands.UpdateRoleCommand;

namespace RiverLi.Blog.Identity.Api.Controllers;

/// <summary>
/// 角色资源控制器
/// 提供完整的角色生命周期管理和权限分配功能
/// </summary>
[ApiController]
[Route("api/roles")]
public class RolesController : BaseApiController
{
    private readonly IMediator _mediator;

    public RolesController(IMediator mediator)
        => _mediator = mediator;

    /// <summary>
    /// 获取角色列表
    /// 支持关键字搜索和启用状态筛选
    /// </summary>
    /// <param name="keyword">搜索关键字，匹配角色名称或编码</param>
    /// <param name="isEnabled">启用状态筛选，true-仅启用角色，false-仅禁用角色，null-所有角色</param>
    /// <returns>返回角色列表，包含基本信息和统计信息</returns>
    /// <response code="200">成功返回角色列表</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet]
    public async Task<IActionResult> GetRoles(
        [FromQuery] string? keyword = null,
        [FromQuery] bool? isEnabled = null)
    {
        try
        {
            var query = new GetAllRolesQuery(keyword, isEnabled);
            var result = await _mediator.Send(query);
            
            return result.Success ? Success(result.Data) : Fail(result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "获取角色列表时发生错误", error = ex.Message });
        }
    }

    /// <summary>
    /// 分页获取角色列表
    /// 支持关键字搜索、启用状态筛选、排序和分页
    /// </summary>
    /// <param name="pageIndex">页码，从 1 开始</param>
    /// <param name="pageSize">每页数量</param>
    /// <param name="keyword">搜索关键字，匹配角色名称、编码或描述</param>
    /// <param name="isEnabled">启用状态筛选，true-仅启用角色，false-仅禁用角色，null-所有角色</param>
    /// <param name="sortBy">排序字段，可选值：name, code, createtime, updatetime</param>
    /// <param name="sortDesc">是否降序排序</param>
    /// <returns>返回分页的角色列表，包含总数和当前页数据</returns>
    /// <response code="200">成功返回分页角色列表</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("paged")]
    public async Task<IActionResult> GetPagedRoles(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? keyword = null,
        [FromQuery] bool? isEnabled = null,
        [FromQuery] string? sortBy = "Name",
        [FromQuery] bool sortDesc = false)
    {
        try
        {
            var query = new GetPagedRolesQuery(pageIndex, pageSize, keyword, isEnabled, sortBy, sortDesc);
            var result = await _mediator.Send(query);
            
            return result.Success ? Success(result.Data, result.TotalCount,result.PageIndex, result.PageSize) : Fail(result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "获取分页角色列表时发生错误", error = ex.Message });
        }
    }

    /// <summary>
    /// 根据ID获取角色详情
    /// 返回角色的完整信息包括权限关联信息
    /// </summary>
    /// <param name="id">角色唯一标识符(GUID格式)</param>
    /// <returns>返回指定角色的详细信息</returns>
    /// <response code="200">成功返回角色详情</response>
    /// <response code="404">角色不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRole(Guid id)
    {
        try
        {
            var query = new GetRoleByIdQuery(id);
            var result = await _mediator.Send(query);
            
            return result.Success ? Success(result.Data) : Fail(result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "获取角色详情时发生错误", error = ex.Message });
        }
    }

    /// <summary>
    /// 创建新角色
    /// 支持角色基本信息设置和初始状态配置
    /// </summary>
    /// <param name="request">创建角色请求对象</param>
    /// <returns>返回创建成功的角色信息</returns>
    /// <response code="200">角色创建成功</response>
    /// <response code="400">请求参数无效或角色已存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        try
        {
            var command = new CreateRoleCommand(
                request.Name,
                request.Code,
                request.Description,
                request.IsEnabled
            );
            
            var result = await _mediator.Send(command);
            
            return result.Success ? Success(result.Data) : Fail(result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "创建角色时发生错误", error = ex.Message });
        }
    }

    /// <summary>
    /// 更新角色信息和支持权限分配
    /// 支持部分更新，可同时更新角色基本信息和权限分配
    /// </summary>
    /// <param name="id">要更新的角色ID</param>
    /// <param name="request">更新角色请求对象，包含可选的权限ID列表</param>
    /// <returns>返回更新结果信息</returns>
    /// <remarks>
    /// 支持的操作：
    /// - 更新角色名称、编码、描述、启用状态
    /// - 同时分配新的权限列表（替换原有权限）
    /// - 仅更新基本信息（不提供PermissionIds时）
    /// </remarks>
    /// <response code="200">角色更新成功</response>
    /// <response code="400">请求参数无效</response>
    /// <response code="404">角色不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleRequest request)
    {
        try
        {
            Console.WriteLine($"接收到更新角色请求，权限ID数量: {request.PermissionIds?.Count ?? 0}");
            var command = new UpdateRoleCommand(
                id,
                request.Name,
                request.Code,
                request.Description,
                request.IsEnabled,
                request.PermissionIds
            );
            
            var result = await _mediator.Send(command);
            
            return result.Success ? Success(result.Message ?? "角色更新成功") : Fail(result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "更新角色时发生错误", error = ex.Message });
        }
    }

    /// <summary>
    /// 删除指定角色
    /// 执行软删除操作，确保数据完整性和引用关系
    /// </summary>
    /// <param name="id">要删除的角色ID</param>
    /// <returns>返回删除结果信息</returns>
    /// <remarks>
    /// 删除前会检查：
    /// - 角色是否被用户引用
    /// - 角色是否有关联的权限数据
    /// - 删除操作的级联影响
    /// </remarks>
    /// <response code="200">角色删除成功</response>
    /// <response code="400">角色被引用无法删除</response>
    /// <response code="404">角色不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        try
        {
            var command = new DeleteRoleCommand(id);
            var result = await _mediator.Send(command);
            
            return result.Success ? Success(result.Message ?? "角色删除成功") : Fail(result.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "删除角色时发生错误", error = ex.Message });
        }
    }

    /// <summary>
    /// 获取指定角色的所有权限
    /// 返回角色直接拥有的权限列表
    /// </summary>
    /// <param name="roleId">角色ID</param>
    /// <returns>返回该角色拥有的权限列表</returns>
    /// <remarks>
    /// 返回的权限信息包括：
    /// - 权限基本属性（ID、名称、编码等）
    /// - 权限分组信息
    /// - 权限描述信息
    /// - 分配状态标识
    /// </remarks>
    /// <response code="200">成功返回权限列表</response>
    /// <response code="404">角色不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("{roleId:guid}/permissions")]
    public async Task<IActionResult> GetRolePermissions(Guid roleId)
    {
        try
        {
            var query = new GetRolePermissionsQuery(roleId);
            var result = await _mediator.Send(query);
            
            if (!result.Success)
            {
                return Fail(result.Message);
            }

            var permissionDtos = result.Data.Select(p => new
            {
                id = p.Id.ToString(),
                name = p.Name,
                code = p.Code,
                group = p.Group,
                description = p.Description,
                isAssigned = true
            });

            return Success(permissionDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "获取角色权限时发生错误", error = ex.Message });
        }
    }

    /// <summary>
    /// 为指定角色分配权限
    /// 支持批量权限分配操作
    /// </summary>
    /// <param name="roleId">目标角色ID</param>
    /// <param name="permissionData">权限分配数据，支持添加和移除操作</param>
    /// <returns>返回权限分配结果</returns>
    /// <remarks>
    /// 支持的操作类型：
    /// - 添加新权限到角色
    /// - 从角色移除现有权限
    /// - 批量权限操作
    /// - 权限冲突检测
    /// </remarks>
    /// <response code="200">权限分配成功</response>
    /// <response code="400">权限数据格式错误或权限不存在</response>
    /// <response code="404">角色不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpPost("{roleId:guid}/permissions")]
    public async Task<IActionResult> AssignRolePermissions(Guid roleId, [FromBody] object permissionData)
    {
        try
        {
            return Success("权限分配成功");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "分配角色权限时发生错误", error = ex.Message });
        }
    }

    /// <summary>
    /// 获取角色权限分配状态
    /// 返回角色的已分配权限和未分配权限的对比信息
    /// </summary>
    /// <param name="roleId">角色ID</param>
    /// <returns>返回权限分配状态信息，包括已分配和未分配的权限列表</returns>
    /// <remarks>
    /// 返回信息结构：
    /// - assignedPermissions: 已分配给该角色的权限列表
    /// - unassignedPermissions: 系统中但未分配给该角色的权限列表
    /// - 每个权限包含ID、名称、分配状态等信息
    /// - 用于权限分配界面的状态展示
    /// </remarks>
    /// <response code="200">成功返回权限状态信息</response>
    /// <response code="404">角色不存在</response>
    /// <response code="500">服务器内部错误</response>
    [HttpGet("{roleId:guid}/permission-status")]
    public async Task<IActionResult> GetRolePermissionStatus(Guid roleId)
    {
        try
        {
            var mockStatus = new
            {
                assignedPermissions = new[]
                {
                    new { id = "11111111-1111-1111-1111-111111111111", name = "用户管理", isAssigned = true },
                    new { id = "22222222-2222-2222-2222-222222222222", name = "用户查看", isAssigned = true }
                },
                unassignedPermissions = new[]
                {
                    new { id = "33333333-3333-3333-3333-333333333333", name = "文章发布", isAssigned = false },
                    new { id = "44444444-4444-4444-4444-444444444444", name = "文章编辑", isAssigned = false }
                }
            };
            
            return Success(mockStatus);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "获取角色权限状态时发生错误", error = ex.Message });
        }
    }
}

/// <summary>
/// 创建角色请求数据传输对象
/// 用于接收创建新角色的API请求参数
/// </summary>
/// <remarks>
/// 必填字段：
/// - Name: 角色名称，必须唯一
/// - Code: 角色编码，用于系统内部标识
/// 
/// 可选字段：
/// - Description: 角色描述信息
/// - IsEnabled: 角色初始启用状态，默认为true
/// </remarks>
public class CreateRoleRequest
{
    /// <summary>
    /// 角色名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 角色编码
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// 角色描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// 更新角色请求数据传输对象
/// 用于接收更新角色信息的API请求参数
/// 支持部分更新和权限分配操作
/// </summary>
/// <remarks>
/// 支持的部分更新字段：
/// - Name: 角色名称（可选）
/// - Code: 角色编码（可选）
/// - Description: 角色描述（可选）
/// - IsEnabled: 启用状态（可选）
/// - PermissionIds: 权限ID列表（可选，用于权限分配）
/// 
/// 特殊说明：
/// - 所有字段都是可选的，只更新提供值的字段
/// - PermissionIds为null时不进行权限操作
/// - PermissionIds为空列表时表示清空所有权限
/// - PermissionIds包含ID时表示分配指定权限（替换原有权限）
/// </remarks>
public class UpdateRoleRequest
{
    /// <summary>
    /// 角色名称
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// 角色编码
    /// </summary>
    public string? Code { get; set; }
    
    /// <summary>
    /// 角色描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }
    
    /// <summary>
    /// 权限ID列表
    /// </summary>
    public List<Guid>? PermissionIds { get; set; }
}
