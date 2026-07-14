// Api/Controllers/ApiResourcesController.cs

using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RiverLi.Blog.Infrastructure.Shared.Controllers;
using RiverLi.Blog.Infrastructure.Shared.OpenApi;
using riverli.blog.services.identity.Application.DTOs.ApiResources;
using riverli.blog.services.identity.Application.Features.ApiResources.Commands;
using riverli.blog.services.identity.Application.Features.ApiResources.Queries;

namespace riverli.blog.services.identity.Api.Controllers;

[ApiController]
[Route("api/identity/[controller]")]
public class ApiResourcesController : BaseApiController
{
    private readonly IMediator _mediator;

    public ApiResourcesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 获取所有 API 资源列表
    /// </summary>
    [HttpGet]
    [Description("获取所有 API 资源列表")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var result = await _mediator.Send(new GetApiResourcesQuery());
            return Success(result);
        }
        catch (Exception ex)
        {
            return Fail($"获取API资源列表失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 1. 获取按微服务分组的 API 权限树 (供前端分配权限界面使用)
    /// </summary>
    [HttpGet("tree")]
    [Description("获取API权限树")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTree()
    {
        try
        {
            var result = await _mediator.Send(new GetApiResourceTreeQuery());
            return Success(result);
        }
        catch (Exception ex)
        {
            return Fail($"获取API权限树失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 2. 主动拉取：一键同步目标微服务的 OpenAPI 文档
    /// (适用于管理员在界面上手动触发某个微服务的同步)
    /// </summary>
    [HttpPost("sync")]
    [Description("同步API资源")]
    public async Task<IActionResult> SyncApis([FromBody] SyncApiRequest request)
    {
        if (string.IsNullOrEmpty(request.ServiceName) || string.IsNullOrEmpty(request.SwaggerUrl))
        {
            return BadRequest(new { success = false, message = "服务名称和 Swagger 地址不能为空" });
        }

        try
        {
            var command = new SyncApisCommand(request.ServiceName, request.SwaggerUrl);
            await _mediator.Send(command);
            return Ok(new { success = true, code = 200, message = $"【{request.ServiceName}】微服务 API 同步成功" });
        }
        catch (HttpRequestException ex)
        {
            return Fail($"无法连接到 Swagger 地址 [{request.SwaggerUrl}]: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Fail($"同步【{request.ServiceName}】API失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 3. 被动接收：接收各大微服务启动时的主动自上报
    /// (适用于微服务集成 Shared 库后的全自动静默上报)
    /// </summary>
    [HttpPost("report")]
    [Description("上报API资源")]
    [AllowAnonymous] // 🌟 极其关键：必须挂上免死金牌！因为微服务刚启动时来上报，手里没有管理员的 JWT Token
    public async Task<IActionResult> ReportApis([FromQuery] string serviceName, [FromBody] List<ReportApiDto> apis)
    {
        if (string.IsNullOrEmpty(serviceName) || apis == null || !apis.Any())
        {
            return BadRequest(new { success = false, message = "上报数据为空或不完整" });
        }

        try
        {
            var command = new ReportApisCommand(serviceName, apis);
            await _mediator.Send(command);
            return Ok(new { success = true, code = 200, message = $"权限中心成功接收【{serviceName}】的上报入库" });
        }
        catch (Exception ex)
        {
            return Fail($"接收【{serviceName}】上报失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 分页查询 API 资源
    /// </summary>
    [HttpGet("page")]
    [Description("分页查询API资源")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPage([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, [FromQuery] string? keyword = null)
    {
        try
        {
            var result = await _mediator.Send(new GetApiResourcesPageQuery(pageIndex, pageSize, keyword));
            return result.Success ? Success(result.Data, result.TotalCount, result.PageIndex, result.PageSize) : Fail(result.Message);
        }
        catch (Exception ex)
        {
            return Fail($"分页查询API资源失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 新增 API 资源
    /// </summary>
    [HttpPost]
    [Description("创建API资源")]
    public async Task<IActionResult> Create([FromBody] CreateOrUpdateApiResourceDto dto)
    {
        try
        {
            var result = await _mediator.Send(new CreateApiResourceCommand(dto));
            return result ? Success("创建成功") : Fail("创建API资源失败");
        }
        catch (Exception ex)
        {
            return Fail($"创建API资源失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 编辑 API 资源
    /// </summary>
    [HttpPut("{id}")]
    [Description("更新API资源")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateOrUpdateApiResourceDto dto)
    {
        try
        {
            var result = await _mediator.Send(new UpdateApiResourceCommand(id, dto));
            return result ? Success("更新成功") : Fail("更新API资源失败");
        }
        catch (Exception ex)
        {
            return Fail($"更新API资源失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 删除 API 资源（同时清理角色-API 关联）
    /// </summary>
    [HttpDelete("{id}")]
    [Description("删除API资源")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new DeleteApiResourceCommand(id));
            return result ? Success("删除成功") : Fail("删除API资源失败");
        }
        catch (Exception ex)
        {
            return Fail($"删除API资源失败: {ex.Message}");
        }
    }
}
