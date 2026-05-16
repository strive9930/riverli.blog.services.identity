using MediatR;
using Microsoft.EntityFrameworkCore;
using RiverLi.Blog.Identity.Infrastructure.Data;
using RiverLi.DDD.Core.Application.Common.Models;

namespace RiverLi.Blog.Identity.Application.Menus.Commands;

/// <summary>
/// 菜单组查询处理器
/// </summary>
public class MenuGroupQueryHandlers
{
    /// <summary>
    /// 分页获取菜单组查询处理器
    /// </summary>
    public class GetPagedMenuGroupsQueryHandler : IRequestHandler<GetPagedMenuGroupsQuery, PagedResult<MenuGroupDto>>
    {
        private readonly IdentityServiceDbContext _context;

        public GetPagedMenuGroupsQueryHandler(IdentityServiceDbContext context)
        {
            this._context = context;
        }
        
        public async Task<PagedResult<MenuGroupDto>> Handle(GetPagedMenuGroupsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var query = _context.MenuGroups
                    .Include(mg => mg.Menus)
                    .AsQueryable();

                // 根据关键词筛选
                if (!string.IsNullOrEmpty(request.Keyword))
                {
                    query = query.Where(mg => 
                        mg.Name.Contains(request.Keyword) || 
                        mg.Code.Contains(request.Keyword) ||
                        (mg.Description != null && mg.Description.Contains(request.Keyword)));
                }

                // 排序
                query = request.SortBy.ToLower() switch
                {
                    "name" => request.SortDesc ? query.OrderByDescending(mg => mg.Name) : query.OrderBy(mg => mg.Name),
                    "code" => request.SortDesc ? query.OrderByDescending(mg => mg.Code) : query.OrderBy(mg => mg.Code),
                    "sort" => request.SortDesc ? query.OrderByDescending(mg => mg.Sort) : query.OrderBy(mg => mg.Sort),
                    "createtime" => request.SortDesc ? query.OrderByDescending(mg => mg.CreateTime) : query.OrderBy(mg => mg.CreateTime),
                    _ => request.SortDesc ? query.OrderByDescending(mg => mg.Sort) : query.OrderBy(mg => mg.Sort)
                };

                // 分页
                var totalCount = await query.CountAsync(cancellationToken);
                var menuGroups = await query
                    .Skip((request.PageIndex - 1) * request.PageSize)
                    .Take(request.PageSize)
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
                    MenuCount = mg.Menus.Count, // 统计关联的菜单数量
                    Menus = mg.Menus.Select(m => new MenuDto
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Title = m.Title,
                        Path = m.Path,
                        Icon = m.Icon,
                        Sort = m.Sort,
                        ParentId = m.ParentId,
                        IsEnabled = m.IsEnabled,
                        IsVisible = m.IsVisible,
                        MenuType = (int)m.MenuType,
                        FrontendRouteId = m.FrontendRouteId,
                        MenuGroupId = m.MenuGroupId,
                        RequiredPermission = m.RequiredPermission,
                        Description = m.Description,
                        Meta = m.Meta,
                        CreateTime = m.CreateTime,
                        UpdateTime = m.UpdateTime
                    }).ToList()
                }).ToList();

                return new PagedResult<MenuGroupDto>
                {
                    Data = dtos,
                    TotalCount = totalCount,
                    PageIndex = request.PageIndex,
                    PageSize = request.PageSize
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching paged menu groups: {ex.Message}");
                throw;
            }
        }
    }
}