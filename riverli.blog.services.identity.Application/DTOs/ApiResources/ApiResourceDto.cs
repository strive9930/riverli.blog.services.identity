using System.ComponentModel.DataAnnotations;

namespace riverli.blog.services.identity.Application.DTOs.ApiResources;

/// <summary>
/// API 资源列表项 DTO
/// </summary>
public class ApiResourceDto
{
    public Guid Id { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
}

/// <summary>
/// 创建/更新 API 资源请求 DTO
/// </summary>
public class CreateOrUpdateApiResourceDto
{
    [Required][StringLength(50)] public string ServiceName { get; init; } = string.Empty;
    [Required][StringLength(10)] public string Method { get; init; } = string.Empty;
    [Required][StringLength(200)] public string Route { get; init; } = string.Empty;
    [StringLength(200)] public string Description { get; init; } = string.Empty;
    public bool IsPublic { get; init; }
}

public class ApiResourceTreeNodeDto
{
    /// <summary>
    /// 节点 ID。如果是分组节点，则为自定义字符串(如 "group_identity")；如果是真实的 API，则为 Guid 字符串
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 节点显示文本
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// 请求方法 (如 GET, POST) - 仅 API 叶子节点有此值
    /// </summary>
    public string? Method { get; set; }

    /// <summary>
    /// 子节点集合
    /// </summary>
    public List<ApiResourceTreeNodeDto> Children { get; set; } = new();
}

/// <summary>
/// 用于 Sync 接口的请求体 DTO
/// </summary>
public class SyncApiRequest
{
    /// <summary>
    /// 微服务名称 (如: IdentityService, BlogService)
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// 该微服务的 Swagger/OpenAPI JSON 访问地址
    /// </summary>
    public string SwaggerUrl { get; set; } = string.Empty;
}
