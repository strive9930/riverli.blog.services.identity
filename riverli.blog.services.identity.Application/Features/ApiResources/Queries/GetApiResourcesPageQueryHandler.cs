using MediatR;
using Microsoft.EntityFrameworkCore;
using riverli.blog.services.identity.Application.DTOs.ApiResources;
using riverli.blog.services.identity.Domain.Entities;
using RiverLi.DDD.Core.Application.Common.Models;
using RiverLi.DDD.Core.Domain.Repositories;

namespace riverli.blog.services.identity.Application.Features.ApiResources.Queries;

public class GetApiResourcesPageQueryHandler : IRequestHandler<GetApiResourcesPageQuery, PagedResult<ApiResourceDto>>
{
    private readonly IRepository<ApiResource, Guid> _repository;

    public GetApiResourcesPageQueryHandler(IRepository<ApiResource, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<ApiResourceDto>> Handle(GetApiResourcesPageQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            query = query.Where(a =>
                a.Route.Contains(request.Keyword) ||
                a.Description.Contains(request.Keyword) ||
                a.ServiceName.Contains(request.Keyword));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(a => a.ServiceName)
            .ThenBy(a => a.Route)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(a => new ApiResourceDto
        {
            Id = a.Id,
            ServiceName = a.ServiceName,
            Method = a.Method,
            Route = a.Route,
            Description = a.Description,
            IsPublic = a.IsPublic
        }).ToList();

        return PagedResult<ApiResourceDto>.SuccessResult(dtos, totalCount, request.PageIndex, request.PageSize);
    }
}
