using MediatR;
using riverli.blog.services.identity.Application.DTOs;

namespace riverli.blog.services.identity.Application.Features.Menus.Queries;

/// <summary>
/// 获取完整菜单树的查询（无需参数）
/// </summary>
public record GetMenuTreeQuery : IRequest<List<MenuTreeDto>>;
