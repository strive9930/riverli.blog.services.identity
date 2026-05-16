using MediatR;
using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Menus.Commands;

/// <summary>
/// 创建菜单组命令处理器
/// </summary>
public class CreateMenuGroupCommandHandler : IRequestHandler<CreateMenuGroupCommand, Result<Guid>>
{
    private readonly IdentityServiceDbContext _context;

    public CreateMenuGroupCommandHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateMenuGroupCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 验证菜单组编码唯一性
            var exists = await _context.MenuGroups
                .AnyAsync(mg => mg.Code == request.Code, cancellationToken);
            if (exists)
            {
                return Result<Guid>.FailResult("菜单组编码已存在");
            }

            var menuGroup = new MenuGroup
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Code = request.Code,
                Description = request.Description,
                Icon = request.Icon,
                Sort = request.Sort,
                IsEnabled = request.IsEnabled,
                CreateTime = DateTime.UtcNow
            };

            _context.MenuGroups.Add(menuGroup);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.SuccessResult(menuGroup.Id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating menu group: {ex.Message}");
            return Result<Guid>.FailResult("创建菜单组失败");
        }
    }
}

/// <summary>
/// 更新菜单组命令处理器
/// </summary>
public class UpdateMenuGroupCommandHandler : IRequestHandler<UpdateMenuGroupCommand, Result<bool>>
{
    private readonly IdentityServiceDbContext _context;

    public UpdateMenuGroupCommandHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(UpdateMenuGroupCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var menuGroup = await _context.MenuGroups
                .FirstOrDefaultAsync(mg => mg.Id == request.Id, cancellationToken);
            
            if (menuGroup == null)
            {
                return Result<bool>.FailResult("菜单组不存在");
            }

            // 验证菜单组编码唯一性 (排除自身)
            if (!string.IsNullOrEmpty(request.Code))
            {
                var exists = await _context.MenuGroups
                    .AnyAsync(mg => mg.Code == request.Code && mg.Id != request.Id, cancellationToken);
                if (exists)
                {
                    return Result<bool>.FailResult("菜单组编码已存在");
                }
            }
            
            // 只更新提供的字段
            if (!string.IsNullOrEmpty(request.Name))
                menuGroup.Name = request.Name;
            if (!string.IsNullOrEmpty(request.Code))
                menuGroup.Code = request.Code;
            if (request.Description != null)
                menuGroup.Description = request.Description;
            if (request.Icon != null)
                menuGroup.Icon = request.Icon;
            menuGroup.Sort = request.Sort;
            menuGroup.IsEnabled = request.IsEnabled;
            menuGroup.UpdateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating menu group {request.Id}: {ex.Message}");
            return Result<bool>.FailResult("更新菜单组失败");
        }
    }
}

/// <summary>
/// 删除菜单组命令处理器
/// </summary>
public class DeleteMenuGroupCommandHandler : IRequestHandler<DeleteMenuGroupCommand, Result<bool>>
{
    private readonly IdentityServiceDbContext _context;

    public DeleteMenuGroupCommandHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(DeleteMenuGroupCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var menuGroup = await _context.MenuGroups
                .Include(mg => mg.Menus)
                .FirstOrDefaultAsync(mg => mg.Id == request.Id, cancellationToken);
            
            if (menuGroup == null)
            {
                return Result<bool>.FailResult("菜单组不存在");
            }

            // 检查是否包含菜单项
            if (menuGroup.Menus.Any())
            {
                return Result<bool>.FailResult("该菜单组下存在菜单项，无法删除");
            }

            _context.MenuGroups.Remove(menuGroup);
            await _context.SaveChangesAsync(cancellationToken);
            
            return Result<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting menu group {request.Id}: {ex.Message}");
            return Result<bool>.FailResult("删除菜单组失败");
        }
    }
}

/// <summary>
/// 获取所有菜单组查询处理器
/// </summary>
public class GetAllMenuGroupsQueryHandler : IRequestHandler<GetAllMenuGroupsQuery, Result<List<MenuGroupDto>>>
{
    private readonly IdentityServiceDbContext _context;

    public GetAllMenuGroupsQueryHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<MenuGroupDto>>> Handle(GetAllMenuGroupsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.MenuGroups
                .Include(mg => mg.Menus)
                .AsQueryable();

            // 根据启用状态筛选
            if (request.IsEnabled.HasValue)
            {
                query = query.Where(mg => mg.IsEnabled == request.IsEnabled.Value);
            }

            // 根据关键词筛选
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(mg => 
                    mg.Name.Contains(request.Keyword) || 
                    mg.Code.Contains(request.Keyword) ||
                    (mg.Description != null && mg.Description.Contains(request.Keyword)));
            }

            var menuGroups = await query
                .OrderBy(mg => mg.Sort)
                .ThenBy(mg => mg.Name)
                .ToListAsync(cancellationToken);

            var dtos = menuGroups.Select(mg => new MenuGroupDto
            {
                Id = mg.Id,
                Name = mg.Name,
                Code = mg.Code,
                Description = mg.Description,
                Icon = mg.Icon,
                Sort = mg.Sort,
                IsEnabled = mg.IsEnabled,
                CreateTime = mg.CreateTime,
                UpdateTime = mg.UpdateTime,
                Menus = mg.Menus.Select(m => new MenuDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Title = m.Title,
                    Path = m.Path
                }).ToList(),
                MenuCount = mg.Menus.Count
            }).ToList();

            return Result<List<MenuGroupDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching menu groups: {ex.Message}");
            return Result<List<MenuGroupDto>>.FailResult("获取菜单组列表失败");
        }
    }
}