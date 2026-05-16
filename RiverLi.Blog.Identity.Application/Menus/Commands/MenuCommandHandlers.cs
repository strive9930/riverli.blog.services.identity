using MediatR;
using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Menus.Commands;

/// <summary>
/// 创建菜单命令处理器
/// </summary>
public class CreateMenuCommandHandler : IRequestHandler<CreateMenuCommand, Result<Guid>>
{
    private readonly IdentityServiceDbContext _context;

    public CreateMenuCommandHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateMenuCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 验证菜单名称唯一性
            var exists = await _context.Menus
                .AnyAsync(m => m.Name == request.Name, cancellationToken);
            if (exists)
            {
                return Result<Guid>.FailResult("菜单名称已存在");
            }

            // 验证父级菜单是否存在
            if (request.ParentId.HasValue)
            {
                var parentExists = await _context.Menus
                    .AnyAsync(m => m.Id == request.ParentId.Value, cancellationToken);
                if (!parentExists)
                {
                    return Result<Guid>.FailResult("父级菜单不存在");
                }
            }

            // 验证菜单组是否存在
            if (request.MenuGroupId.HasValue)
            {
                var groupExists = await _context.MenuGroups
                    .AnyAsync(mg => mg.Id == request.MenuGroupId.Value, cancellationToken);
                if (!groupExists)
                {
                    return Result<Guid>.FailResult("菜单组不存在");
                }
            }

            // 验证前端路由是否存在
            if (request.FrontendRouteId.HasValue)
            {
                var routeExists = await _context.FrontendRoutes
                    .AnyAsync(fr => fr.Id == request.FrontendRouteId.Value, cancellationToken);
                if (!routeExists)
                {
                    return Result<Guid>.FailResult("前端路由不存在");
                }
            }

            var menu = new Menu
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Title = request.Title,
                Path = request.Path,
                Icon = request.Icon,
                Sort = request.Sort,
                IsEnabled = request.IsEnabled,
                IsVisible = request.IsVisible,
                ParentId = request.ParentId,
                MenuType = (MenuType)request.MenuType,
                FrontendRouteId = request.FrontendRouteId,
                MenuGroupId = request.MenuGroupId,
                RequiredPermission = request.RequiredPermission,
                Description = request.Description,
                Meta = request.Meta,
                CreateTime = DateTime.UtcNow
            };

            _context.Menus.Add(menu);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.SuccessResult(menu.Id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating menu: {ex.Message}");
            return Result<Guid>.FailResult("创建菜单失败");
        }
    }
}

/// <summary>
/// 更新菜单命令处理器
/// </summary>
public class UpdateMenuCommandHandler : IRequestHandler<UpdateMenuCommand, Result<bool>>
{
    private readonly IdentityServiceDbContext _context;

    public UpdateMenuCommandHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(UpdateMenuCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var menu = await _context.Menus
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
            
            if (menu == null)
            {
                return Result<bool>.FailResult("菜单不存在");
            }

            // 验证菜单名称唯一性（排除自身）
            var exists = await _context.Menus
                .AnyAsync(m => m.Name == request.Name && m.Id != request.Id, cancellationToken);
            if (exists)
            {
                return Result<bool>.FailResult("菜单名称已存在");
            }

            // 验证父级菜单是否存在
            if (request.ParentId.HasValue && request.ParentId.Value != menu.ParentId)
            {
                var parentExists = await _context.Menus
                    .AnyAsync(m => m.Id == request.ParentId.Value, cancellationToken);
                if (!parentExists)
                {
                    return Result<bool>.FailResult("父级菜单不存在");
                }
            }

            // 验证菜单组是否存在
            if (request.MenuGroupId.HasValue && request.MenuGroupId.Value != menu.MenuGroupId)
            {
                var groupExists = await _context.MenuGroups
                    .AnyAsync(mg => mg.Id == request.MenuGroupId.Value, cancellationToken);
                if (!groupExists)
                {
                    return Result<bool>.FailResult("菜单组不存在");
                }
            }

            // 验证前端路由是否存在
            if (request.FrontendRouteId.HasValue && request.FrontendRouteId.Value != menu.FrontendRouteId)
            {
                var routeExists = await _context.FrontendRoutes
                    .AnyAsync(fr => fr.Id == request.FrontendRouteId.Value, cancellationToken);
                if (!routeExists)
                {
                    return Result<bool>.FailResult("前端路由不存在");
                }
            }

            // 更新菜单属性
            menu.Name = request.Name;
            menu.Title = request.Title;
            menu.Path = request.Path;
            menu.Icon = request.Icon;
            menu.Sort = request.Sort;
            menu.IsEnabled = request.IsEnabled;
            menu.IsVisible = request.IsVisible;
            menu.ParentId = request.ParentId;
            menu.MenuType = (MenuType)request.MenuType;
            menu.FrontendRouteId = request.FrontendRouteId;
            menu.MenuGroupId = request.MenuGroupId;
            menu.RequiredPermission = request.RequiredPermission;
            menu.Description = request.Description;
            menu.Meta = request.Meta;
            menu.UpdateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating menu {request.Id}: {ex.Message}");
            return Result<bool>.FailResult("更新菜单失败");
        }
    }
}

/// <summary>
/// 删除菜单命令处理器
/// </summary>
public class DeleteMenuCommandHandler : IRequestHandler<DeleteMenuCommand, Result<bool>>
{
    private readonly IdentityServiceDbContext _context;

    public DeleteMenuCommandHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(DeleteMenuCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var menu = await _context.Menus
                .Include(m => m.Children)
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
            
            if (menu == null)
            {
                return Result<bool>.FailResult("菜单不存在");
            }

            // 检查是否包含子菜单
            if (menu.Children.Any())
            {
                return Result<bool>.FailResult("该菜单下存在子菜单，无法删除");
            }

            _context.Menus.Remove(menu);
            await _context.SaveChangesAsync(cancellationToken);
            
            return Result<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting menu {request.Id}: {ex.Message}");
            return Result<bool>.FailResult("删除菜单失败");
        }
    }
}

/// <summary>
/// 更新菜单排序命令处理器
/// </summary>
public class UpdateMenuSortCommandHandler : IRequestHandler<UpdateMenuSortCommand, Result<bool>>
{
    private readonly IdentityServiceDbContext _context;

    public UpdateMenuSortCommandHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(UpdateMenuSortCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 获取所有需要更新排序的菜单
            var menus = await _context.Menus
                .Where(m => request.MenuIds.Contains(m.Id))
                .ToListAsync(cancellationToken);

            // 更新排序
            for (int i = 0; i < request.MenuIds.Count && i < menus.Count; i++)
            {
                var menu = menus.FirstOrDefault(m => m.Id == request.MenuIds[i]);
                if (menu != null)
                {
                    menu.Sort = i + 1;
                    menu.UpdateTime = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating menu sort: {ex.Message}");
            return Result<bool>.FailResult("更新菜单排序失败");
        }
    }
}

/// <summary>
/// 批量删除菜单命令处理器
/// </summary>
public class BatchDeleteMenusCommandHandler : IRequestHandler<BatchDeleteMenusCommand, Result<bool>>
{
    private readonly IdentityServiceDbContext _context;

    public BatchDeleteMenusCommandHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(BatchDeleteMenusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var menus = await _context.Menus
                .Include(m => m.Children)
                .Where(m => request.MenuIds.Contains(m.Id))
                .ToListAsync(cancellationToken);

            // 检查是否有菜单包含子菜单
            var menuWithChildren = menus.FirstOrDefault(m => m.Children.Any());
            if (menuWithChildren != null)
            {
                return Result<bool>.FailResult($"菜单 '{menuWithChildren.Name}' 下存在子菜单，无法删除");
            }

            _context.Menus.RemoveRange(menus);
            await _context.SaveChangesAsync(cancellationToken);
            
            return Result<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error batch deleting menus: {ex.Message}");
            return Result<bool>.FailResult("批量删除菜单失败");
        }
    }
}

/// <summary>
/// 批量更新菜单状态命令处理器
/// </summary>
public class BatchUpdateMenuStatusCommandHandler : IRequestHandler<BatchUpdateMenuStatusCommand, Result<bool>>
{
    private readonly IdentityServiceDbContext _context;

    public BatchUpdateMenuStatusCommandHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(BatchUpdateMenuStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var menus = await _context.Menus
                .Where(m => request.MenuIds.Contains(m.Id))
                .ToListAsync(cancellationToken);

            foreach (var menu in menus)
            {
                menu.IsEnabled = request.IsEnabled;
                menu.UpdateTime = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error batch updating menu status: {ex.Message}");
            return Result<bool>.FailResult("批量更新菜单状态失败");
        }
    }
}