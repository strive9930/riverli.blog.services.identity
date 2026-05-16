using MediatR;
using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.FrontendRoutes.Commands;

/// <summary>
/// 创建前端路由命令处理器
/// </summary>
public class CreateFrontendRouteCommandHandler : IRequestHandler<CreateFrontendRouteCommand, Result<Guid>>
{
    private readonly IdentityServiceDbContext _context;

    public CreateFrontendRouteCommandHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateFrontendRouteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 验证父级路由是否存在
            if (request.ParentId.HasValue)
            {
                var parentExists = await _context.FrontendRoutes
                    .AnyAsync(r => r.Id == request.ParentId.Value, cancellationToken);
                if (!parentExists)
                {
                    return Result<Guid>.FailResult("指定的父级路由不存在");
                }
            }

            // 验证路由路径唯一性
            var pathExists = await _context.FrontendRoutes
                .AnyAsync(r => r.Path == request.Path, cancellationToken);
            if (pathExists)
            {
                return Result<Guid>.FailResult("路由路径已存在");
            }

            var route = FrontendRoute.Create(
                request.Path,
                request.Name,
                request.Component,
                request.Title,
                request.Sort,
                request.ParentId,
                request.Icon,
                request.IsMenu,
                request.RequiredPermission,
                request.RouteGroupId
            );

            route.IsEnabled = request.IsEnabled;
            route.Description = request.Description;
            route.Meta = request.Meta;
            route.RouteGroupId = request.RouteGroupId;

            _context.FrontendRoutes.Add(route);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.SuccessResult(route.Id, "前端路由创建成功");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating frontend route: {ex.Message}");
            return Result<Guid>.FailResult("前端路由创建失败");
        }
    }
}

/// <summary>
/// 更新前端路由命令处理器
/// </summary>
public class UpdateFrontendRouteCommandHandler : IRequestHandler<UpdateFrontendRouteCommand, Result<bool>>
{
    private readonly IdentityServiceDbContext _context;

    public UpdateFrontendRouteCommandHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(UpdateFrontendRouteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var route = await _context.FrontendRoutes
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (route == null)
            {
                return Result<bool>.FailResult("前端路由不存在");
            }

            // 验证父级路由
            if (request.ParentId.HasValue && request.ParentId != route.ParentId)
            {
                var parentExists = await _context.FrontendRoutes
                    .AnyAsync(r => r.Id == request.ParentId.Value, cancellationToken);
                if (!parentExists)
                {
                    return Result<bool>.FailResult("指定的父级路由不存在");
                }
            }

            // 验证路径唯一性（排除自身）
            var pathExists = await _context.FrontendRoutes
                .AnyAsync(r => r.Path == request.Path && r.Id != request.Id, cancellationToken);
            if (pathExists)
            {
                return Result<bool>.FailResult("路由路径已存在");
            }

            Console.WriteLine($"Updating route {route.Id}: Current RouteGroupId = {route.RouteGroupId}, New RouteGroupId = {request.RouteGroupId}");
            
            // 只更新已提供的字段（非 null 的字段）
            if (request.Path != null)
                route.Path = request.Path;
            if (request.Name != null)
                route.Name = request.Name;
            if (request.Component != null)
                route.Component = request.Component;
            if (request.Title != null)
                route.Title = request.Title;
            if (request.Icon != null)
                route.Icon = request.Icon;
            if (request.Sort.HasValue)
                route.Sort = request.Sort.Value;
            if (request.ParentId != null)
                route.ParentId = request.ParentId;
            if (request.IsMenu.HasValue)
                route.IsMenu = request.IsMenu.Value;
            if (request.IsEnabled.HasValue)
                route.IsEnabled = request.IsEnabled.Value;
            if (request.RequiredPermission != null)
                route.RequiredPermission = request.RequiredPermission;
            if (request.Description != null)
                route.Description = request.Description;
            if (request.Meta != null)
                route.Meta = request.Meta;
            if (request.RouteGroupId != null)
                route.RouteGroupId = request.RouteGroupId;
            
            route.UpdateTime = DateTime.UtcNow;
            
            Console.WriteLine($"After setting: RouteGroupId = {route.RouteGroupId}");

            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.SuccessResult(true, "前端路由更新成功");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating frontend route {request.Id}: {ex.Message}");
            return Result<bool>.FailResult("前端路由更新失败");
        }
    }
}

/// <summary>
/// 删除前端路由命令处理器
/// </summary>
public class DeleteFrontendRouteCommandHandler : IRequestHandler<DeleteFrontendRouteCommand, Result<bool>>
{
    private readonly IdentityServiceDbContext _context;

    public DeleteFrontendRouteCommandHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(DeleteFrontendRouteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var route = await _context.FrontendRoutes
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (route == null)
            {
                return Result<bool>.FailResult("前端路由不存在");
            }

            // 检查是否有子路由
            var hasChildren = await _context.FrontendRoutes
                .AnyAsync(r => r.ParentId == request.Id, cancellationToken);
            if (hasChildren)
            {
                return Result<bool>.FailResult("该路由存在子路由，无法删除");
            }

            _context.FrontendRoutes.Remove(route);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.SuccessResult(true, "前端路由删除成功");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting frontend route {request.Id}: {ex.Message}");
            return Result<bool>.FailResult("前端路由删除失败");
        }
    }
}