using MediatR;
using riverli.blog.services.identity.Application.DTOs;
using riverli.blog.services.identity.Application.Interfaces; // 引入刚刚定义的接口

namespace riverli.blog.services.identity.Application.Features.Menus.Queries;

public record GetUserMenusQuery(Guid UserId) : IRequest<List<MenuDto>>;

