using MediatR;
using riverli.blog.services.identity.Application.DTOs;

namespace riverli.blog.services.identity.Application.Features.Menus.Commands;

// --- 创建菜单 ---
public record CreateMenuCommand(CreateOrUpdateMenuDto Data) : IRequest<bool>;