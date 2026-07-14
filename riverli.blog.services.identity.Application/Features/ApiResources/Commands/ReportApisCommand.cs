using MediatR;
using RiverLi.Blog.Infrastructure.Shared.OpenApi;

namespace riverli.blog.services.identity.Application.Features.ApiResources.Commands;

public record ReportApisCommand(string ServiceName, List<ReportApiDto> Apis) : IRequest<bool>;