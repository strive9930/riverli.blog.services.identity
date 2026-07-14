using MediatR;
using riverli.blog.services.identity.Application.DTOs;

namespace riverli.blog.services.identity.Application.Features.Menus.Commands;

// --- 更新菜单 ---
public record UpdateMenuCommand(Guid Id, CreateOrUpdateMenuDto Data) : IRequest<bool>;