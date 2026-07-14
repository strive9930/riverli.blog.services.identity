using MediatR;

namespace riverli.blog.services.identity.Application.Features.ApiResources.Commands;

public record SyncApisCommand(string ServiceName, string SwaggerUrl) : IRequest<bool>;