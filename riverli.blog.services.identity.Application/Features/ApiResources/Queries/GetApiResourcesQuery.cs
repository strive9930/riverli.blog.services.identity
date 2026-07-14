using MediatR;
using riverli.blog.services.identity.Application.DTOs.ApiResources;

namespace riverli.blog.services.identity.Application.Features.ApiResources.Queries;

public record GetApiResourcesQuery : IRequest<List<ApiResourceDto>>;
