using MediatR;
using riverli.blog.services.identity.Application.DTOs.ApiResources;
using RiverLi.DDD.Core.Application.Common.Models;

namespace riverli.blog.services.identity.Application.Features.ApiResources.Queries;

public record GetApiResourcesPageQuery(int PageIndex, int PageSize, string? Keyword) : IRequest<PagedResult<ApiResourceDto>>;
