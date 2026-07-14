using MediatR;

namespace riverli.blog.services.identity.Application.Features.Users.Queries;

/// <summary>
/// 查询当前用户拥有哪些后端 API 接口权限
/// 返回格式: ["GET:/api/users", "POST:/api/users", ...]
/// </summary>
public record GetMyPermissionsQuery(Guid UserId) : IRequest<List<string>>;
