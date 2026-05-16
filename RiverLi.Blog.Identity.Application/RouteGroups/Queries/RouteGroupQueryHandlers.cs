using MediatR;
using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Domain.Entities;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;
using RiverLi.Blog.Identity.Application.RouteGroups.Commands;

namespace RiverLi.Blog.Identity.Application.RouteGroups.Queries;

/// <summary>
/// 获取路由分组详情查询处理器
/// </summary>
public class GetRouteGroupByIdQueryHandler : IRequestHandler<GetRouteGroupByIdQuery, Result<RouteGroupDto>>
{
    private readonly IdentityServiceDbContext _context;

    public GetRouteGroupByIdQueryHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<RouteGroupDto>> Handle(GetRouteGroupByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var routeGroup = await _context.RouteGroups
                .Include(rg => rg.Children)
                .Include(rg => rg.Parent)
                .Include(rg => rg.FrontendRoutes)
                .Include(rg => rg.BackendRoutes)
                .FirstOrDefaultAsync(rg => rg.Id == request.Id, cancellationToken);

            if (routeGroup == null)
            {
                return Result<RouteGroupDto>.FailResult("路由分组不存在");
            }

            var dto = MapToDto(routeGroup);
            return Result<RouteGroupDto>.SuccessResult(dto);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching route group {request.Id}: {ex.Message}");
            return Result<RouteGroupDto>.FailResult("获取路由分组失败");
        }
    }

    private RouteGroupDto MapToDto(RouteGroup routeGroup)
    {
        return new RouteGroupDto
        {
            Id = routeGroup.Id,
            Name = routeGroup.Name,
            Code = routeGroup.Code,
            Description = routeGroup.Description,
            Icon = routeGroup.Icon,
            Sort = routeGroup.Sort,
            IsEnabled = routeGroup.IsEnabled,
            ParentId = routeGroup.ParentId,
            GroupType = (int)routeGroup.GroupType,
            RequiredPermission = routeGroup.RequiredPermission,
            CreateTime = routeGroup.CreateTime,
            UpdateTime = routeGroup.UpdateTime,
            Parent = routeGroup.Parent != null ? MapToDto(routeGroup.Parent) : null,
            Children = routeGroup.Children.Select(MapToDto).ToList(),
            FrontendRouteCount = routeGroup.FrontendRoutes.Count,
            BackendRouteCount = routeGroup.BackendRoutes.Count
        };
    }
}

/// <summary>
/// 获取所有路由分组查询处理器
/// </summary>
public class GetAllRouteGroupsQueryHandler : IRequestHandler<GetAllRouteGroupsQuery, Result<List<RouteGroupDto>>>
{
    private readonly IdentityServiceDbContext _context;

    public GetAllRouteGroupsQueryHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<RouteGroupDto>>> Handle(GetAllRouteGroupsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.RouteGroups
                .Include(rg => rg.Children)
                .Include(rg => rg.Parent)
                .Include(rg => rg.FrontendRoutes)
                .Include(rg => rg.BackendRoutes)
                .AsQueryable();

            // 根据分组类型筛选
            if (request.GroupType.HasValue)
            {
                query = query.Where(rg => (int)rg.GroupType == request.GroupType.Value);
            }

            // 根据启用状态筛选
            if (request.IsEnabled.HasValue)
            {
                query = query.Where(rg => rg.IsEnabled == request.IsEnabled.Value);
            }

            // 根据关键词筛选
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(rg => 
                    rg.Name.Contains(request.Keyword) || 
                    rg.Code.Contains(request.Keyword) ||
                    (rg.Description != null && rg.Description.Contains(request.Keyword)));
            }

            var routeGroups = await query
                .OrderBy(rg => rg.Sort)
                .ThenBy(rg => rg.Name)
                .ToListAsync(cancellationToken);

            var dtos = routeGroups.Select(rg => new RouteGroupDto
            {
                Id = rg.Id,
                Name = rg.Name,
                Code = rg.Code,
                Description = rg.Description,
                Icon = rg.Icon,
                Sort = rg.Sort,
                IsEnabled = rg.IsEnabled,
                ParentId = rg.ParentId,
                GroupType = (int)rg.GroupType,
                RequiredPermission = rg.RequiredPermission,
                CreateTime = rg.CreateTime,
                UpdateTime = rg.UpdateTime,
                Parent = rg.Parent != null ? new RouteGroupDto
                {
                    Id = rg.Parent.Id,
                    Name = rg.Parent.Name,
                    Code = rg.Parent.Code
                } : null,
                Children = rg.Children.Select(child => new RouteGroupDto
                {
                    Id = child.Id,
                    Name = child.Name,
                    Code = child.Code
                }).ToList(),
                FrontendRouteCount = rg.FrontendRoutes.Count,
                BackendRouteCount = rg.BackendRoutes.Count
            }).ToList();

            return Result<List<RouteGroupDto>>.SuccessResult(dtos);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching route groups: {ex.Message}");
            return Result<List<RouteGroupDto>>.FailResult("获取路由分组列表失败");
        }
    }
}

/// <summary>
/// 分页获取路由分组查询处理器
/// </summary>
public class GetPagedRouteGroupsQueryHandler : IRequestHandler<GetPagedRouteGroupsQuery, PagedResult<RouteGroupDto>>
{
    private readonly IdentityServiceDbContext _context;

    public GetPagedRouteGroupsQueryHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<RouteGroupDto>> Handle(GetPagedRouteGroupsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.RouteGroups.AsQueryable();

            // 应用筛选条件
            if (request.GroupType.HasValue)
            {
                query = query.Where(rg => (int)rg.GroupType == request.GroupType.Value);
            }

            if (request.IsEnabled.HasValue)
            {
                query = query.Where(rg => rg.IsEnabled == request.IsEnabled.Value);
            }

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(rg => 
                    rg.Name.Contains(request.Keyword) || 
                    rg.Code.Contains(request.Keyword) ||
                    (rg.Description != null && rg.Description.Contains(request.Keyword)));
            }

            // 获取总数
            var totalCount = await query.CountAsync(cancellationToken);

            // 应用排序
            if (!string.IsNullOrEmpty(request.SortBy))
            {
                query = request.SortBy.ToLower() switch
                {
                    "name" => request.SortDesc ? query.OrderByDescending(rg => rg.Name) : query.OrderBy(rg => rg.Name),
                    "code" => request.SortDesc ? query.OrderByDescending(rg => rg.Code) : query.OrderBy(rg => rg.Code),
                    "createtime" => request.SortDesc ? query.OrderByDescending(rg => rg.CreateTime) : query.OrderBy(rg => rg.CreateTime),
                    _ => request.SortDesc ? query.OrderByDescending(rg => rg.Sort) : query.OrderBy(rg => rg.Sort)
                };
            }
            else
            {
                query = query.OrderBy(rg => rg.Sort);
            }

            // 分页查询
            var routeGroups = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var dtos = routeGroups.Select(rg => new RouteGroupDto
            {
                Id = rg.Id,
                Name = rg.Name,
                Code = rg.Code,
                Description = rg.Description,
                Icon = rg.Icon,
                Sort = rg.Sort,
                IsEnabled = rg.IsEnabled,
                ParentId = rg.ParentId,
                GroupType = (int)rg.GroupType,
                RequiredPermission = rg.RequiredPermission,
                CreateTime = rg.CreateTime,
                UpdateTime = rg.UpdateTime
            }).ToList();

            /*var pagedResult = PagedResult<RouteGroupDto>.Create(
                new PagedQuery { PageIndex = request.PageIndex, PageSize = request.PageSize },
                totalCount,
                dtos
            );*/

            return PagedResult<RouteGroupDto>.SuccessResult(dtos,totalCount,request.PageIndex,request.PageSize);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching paged route groups: {ex.Message}");
            return PagedResult<RouteGroupDto>.FailResult("获取分页路由分组列表失败");
        }
    }
}

/// <summary>
/// 获取路由分组树结构查询处理器
/// </summary>
public class GetRouteGroupTreeQueryHandler : IRequestHandler<GetRouteGroupTreeQuery, Result<List<RouteGroupTreeNodeDto>>>
{
    private readonly IdentityServiceDbContext _context;

    public GetRouteGroupTreeQueryHandler(IdentityServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<RouteGroupTreeNodeDto>>> Handle(GetRouteGroupTreeQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.RouteGroups.AsQueryable();

            // 根据分组类型筛选
            if (request.GroupType.HasValue)
            {
                query = query.Where(rg => (int)rg.GroupType == request.GroupType.Value);
            }

            // 根据启用状态筛选
            if (request.IsEnabled.HasValue)
            {
                query = query.Where(rg => rg.IsEnabled == request.IsEnabled.Value);
            }

            var allGroups = await query
                .OrderBy(rg => rg.Sort)
                .ThenBy(rg => rg.Name)
                .ToListAsync(cancellationToken);

            // 构建树形结构
            var treeNodes = BuildTree(allGroups.Where(rg => !rg.ParentId.HasValue).ToList(), allGroups);
            
            return Result<List<RouteGroupTreeNodeDto>>.SuccessResult(treeNodes);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching route group tree: {ex.Message}");
            return Result<List<RouteGroupTreeNodeDto>>.FailResult("获取路由分组树失败");
        }
    }

    private List<RouteGroupTreeNodeDto> BuildTree(List<RouteGroup> rootGroups, List<RouteGroup> allGroups)
    {
        var nodes = new List<RouteGroupTreeNodeDto>();
        
        foreach (var group in rootGroups)
        {
            var node = new RouteGroupTreeNodeDto
            {
                Id = group.Id,
                Name = group.Name,
                Code = group.Code,
                Icon = group.Icon,
                Sort = group.Sort,
                IsEnabled = group.IsEnabled,
                ParentId = group.ParentId
            };

            // 递归构建子节点
            var children = allGroups.Where(g => g.ParentId == group.Id).ToList();
            if (children.Any())
            {
                node.Children = BuildTree(children, allGroups);
            }

            nodes.Add(node);
        }

        return nodes;
    }
}