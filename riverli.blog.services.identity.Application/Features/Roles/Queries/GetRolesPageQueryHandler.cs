using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using riverli.blog.services.identity.Application.DTOs.Roles;
using riverli.blog.services.identity.Domain.Entities;
using RiverLi.DDD.Core.Application.Common.Models;

namespace riverli.blog.services.identity.Application.Features.Roles.Queries;

public class GetRolesPageQueryHandler : IRequestHandler<GetRolesPageQuery, PagedResult<RoleDto>>
{
    private readonly RoleManager<AppRole> _roleManager;

    public GetRolesPageQueryHandler(RoleManager<AppRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<PagedResult<RoleDto>> Handle(GetRolesPageQuery request, CancellationToken cancellationToken)
    {
        var query = _roleManager.Roles.AsQueryable();

        // 按名称关键字过滤
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            query = query.Where(r => r.Name!.Contains(request.Keyword));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var roles = await query
            .OrderBy(r => r.Name)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var roleDtos = roles.Select(r => new RoleDto(r.Id, r.Name!, r.Description)).ToList();

        return PagedResult<RoleDto>.SuccessResult(roleDtos, totalCount, request.PageIndex, request.PageSize);
    }
}
