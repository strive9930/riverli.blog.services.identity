using MediatR;

namespace riverli.blog.services.identity.Application.Features.Menus.Commands;

// --- 删除菜单 ---
public record DeleteMenuCommand(Guid Id) : IRequest<bool>;