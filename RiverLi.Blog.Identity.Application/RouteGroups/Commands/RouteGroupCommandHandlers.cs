using MediatR;
using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.RouteGroups.Commands;

/// <summary>
/// 创建路由分组命令处理器
/// </summary>
public class CreateRouteGroupCommandHandler : IRequestHandler<CreateRouteGroupCommand, Result<Guid>>
{
    private readonly IdentityServiceDbContext _context;

    public CreateRouteGroupCommandHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateRouteGroupCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 验证分组编码唯一性
            var exists = await _context.RouteGroups
                .AnyAsync(rg => rg.Code == request.Code, cancellationToken);
            if (exists)
            {
                return Result<Guid>.FailResult("分组编码已存在");
            }

            // 验证父级分组是否存在
            if (request.ParentId.HasValue)
            {
                var parentExists = await _context.RouteGroups
                    .AnyAsync(rg => rg.Id == request.ParentId.Value, cancellationToken);
                if (!parentExists)
                {
                    return Result<Guid>.FailResult("指定的父级分组不存在");
                }
            }

            var routeGroup = new RouteGroup
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Code = request.Code,
                Description = request.Description,
                Icon = request.Icon,
                Sort = request.Sort,
                ParentId = request.ParentId,
                IsEnabled = request.IsEnabled,
                GroupType = (RouteGroupType)request.GroupType,
                RequiredPermission = request.RequiredPermission,
                CreateTime = DateTime.UtcNow
            };

            _context.RouteGroups.Add(routeGroup);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.SuccessResult(routeGroup.Id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating route group: {ex.Message}");
            return Result<Guid>.FailResult("创建路由分组失败");
        }
    }
}

/// <summary>
/// 更新路由分组命令处理器
/// </summary>
public class UpdateRouteGroupCommandHandler : IRequestHandler<UpdateRouteGroupCommand, Result<bool>>
{
    private readonly IdentityServiceDbContext _context;

    public UpdateRouteGroupCommandHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(UpdateRouteGroupCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var routeGroup = await _context.RouteGroups
                .FirstOrDefaultAsync(rg => rg.Id == request.Id, cancellationToken);
            
            if (routeGroup == null)
            {
                return Result<bool>.FailResult("路由分组不存在");
            }

            // 验证分组编码唯一性（排除自身）
            var exists = await _context.RouteGroups
                .AnyAsync(rg => rg.Code == request.Code && rg.Id != request.Id, cancellationToken);
            if (exists)
            {
                return Result<bool>.FailResult("分组编码已存在");
            }

            // 验证父级分组是否存在（排除自身作为父级）
            if (request.ParentId.HasValue && request.ParentId.Value != routeGroup.Id)
            {
                var parentExists = await _context.RouteGroups
                    .AnyAsync(rg => rg.Id == request.ParentId.Value, cancellationToken);
                if (!parentExists)
                {
                    return Result<bool>.FailResult("指定的父级分组不存在");
                }
            }

            routeGroup.Name = request.Name ?? routeGroup.Name;
            routeGroup.Code = request.Code ?? routeGroup.Code;
            routeGroup.Description = request.Description ?? routeGroup.Description;
            routeGroup.Icon = request.Icon ?? routeGroup.Icon;
            routeGroup.Sort = request.Sort ?? routeGroup.Sort;
            routeGroup.ParentId = request.ParentId ?? routeGroup.ParentId;
            routeGroup.IsEnabled = request.IsEnabled ?? routeGroup.IsEnabled;
            if (request.GroupType.HasValue)
                routeGroup.GroupType = (RouteGroupType)request.GroupType.Value;
            routeGroup.RequiredPermission = request.RequiredPermission ?? routeGroup.RequiredPermission;
            routeGroup.UpdateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating route group {request.Id}: {ex.Message}");
            return Result<bool>.FailResult("更新路由分组失败");
        }
    }
}

/// <summary>
/// 删除路由分组命令处理器
/// </summary>
public class DeleteRouteGroupCommandHandler : IRequestHandler<DeleteRouteGroupCommand, Result<bool>>
{
    private readonly IdentityServiceDbContext _context;

    public DeleteRouteGroupCommandHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(DeleteRouteGroupCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var routeGroup = await _context.RouteGroups
                .Include(rg => rg.Children)
                .FirstOrDefaultAsync(rg => rg.Id == request.Id, cancellationToken);
            
            if (routeGroup == null)
            {
                return Result<bool>.FailResult("路由分组不存在");
            }

            // 检查是否有子分组
            if (routeGroup.Children.Any())
            {
                return Result<bool>.FailResult("该分组下存在子分组，无法删除");
            }

            // 检查是否有关联的路由
            var hasFrontendRoutes = await _context.FrontendRoutes
                .AnyAsync(fr => fr.RouteGroupId == request.Id, cancellationToken);
            var hasBackendRoutes = await _context.SysRoutes
                .AnyAsync(sr => sr.RouteGroupId == request.Id, cancellationToken);
            
            if (hasFrontendRoutes || hasBackendRoutes)
            {
                return Result<bool>.FailResult("该分组下存在关联路由，无法删除");
            }

            _context.RouteGroups.Remove(routeGroup);
            await _context.SaveChangesAsync(cancellationToken);
            
            return Result<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting route group {request.Id}: {ex.Message}");
            return Result<bool>.FailResult("删除路由分组失败");
        }
    }
}

/// <summary>
/// 批量删除路由分组命令处理器
/// </summary>
public class BatchDeleteRouteGroupsCommandHandler : IRequestHandler<BatchDeleteRouteGroupsCommand, Result<int>>
{
    private readonly IdentityServiceDbContext _context;

    public BatchDeleteRouteGroupsCommandHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(BatchDeleteRouteGroupsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var routeGroups = await _context.RouteGroups
                .Include(rg => rg.Children)
                .Where(rg => request.RouteGroupIds.Contains(rg.Id))
                .ToListAsync(cancellationToken);

            if (!routeGroups.Any())
            {
                return Result<int>.FailResult("未找到指定的路由分组");
            }

            // 检查是否有子分组
            var groupsWithChildren = routeGroups.Where(rg => rg.Children.Any()).ToList();
            if (groupsWithChildren.Any())
            {
                var names = string.Join(", ", groupsWithChildren.Select(rg => rg.Name));
                return Result<int>.FailResult($"以下分组存在子分组，无法删除: {names}");
            }

            // 检查是否有关联的路由
            var groupIds = routeGroups.Select(rg => rg.Id).ToList();
            var hasFrontendRoutes = await _context.FrontendRoutes
                .AnyAsync(fr => groupIds.Contains(fr.RouteGroupId!.Value), cancellationToken);
            var hasBackendRoutes = await _context.SysRoutes
                .AnyAsync(sr => groupIds.Contains(sr.RouteGroupId!.Value), cancellationToken);
            
            if (hasFrontendRoutes || hasBackendRoutes)
            {
                return Result<int>.FailResult("选中的分组中存在关联路由，无法删除");
            }

            _context.RouteGroups.RemoveRange(routeGroups);
            await _context.SaveChangesAsync(cancellationToken);
            
            return Result<int>.SuccessResult(routeGroups.Count);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error batch deleting route groups: {ex.Message}");
            return Result<int>.FailResult("批量删除路由分组失败");
        }
    }
}

/// <summary>
/// 批量启用/禁用路由分组命令处理器
/// </summary>
public class BatchToggleRouteGroupsCommandHandler : IRequestHandler<BatchToggleRouteGroupsCommand, Result<int>>
{
    private readonly IdentityServiceDbContext _context;

    public BatchToggleRouteGroupsCommandHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(BatchToggleRouteGroupsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var routeGroups = await _context.RouteGroups
                .Where(rg => request.Ids.Contains(rg.Id))
                .ToListAsync(cancellationToken);

            if (!routeGroups.Any())
            {
                return Result<int>.FailResult("未找到指定的路由分组");
            }

            foreach (var routeGroup in routeGroups)
            {
                routeGroup.IsEnabled = request.IsEnabled;
                routeGroup.UpdateTime = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result<int>.SuccessResult(routeGroups.Count);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error batch toggling route groups: {ex.Message}");
            return Result<int>.FailResult("批量启用/禁用路由分组失败");
        }
    }
}