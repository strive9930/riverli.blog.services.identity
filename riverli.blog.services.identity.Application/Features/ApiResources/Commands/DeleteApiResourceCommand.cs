using MediatR;

namespace riverli.blog.services.identity.Application.Features.ApiResources.Commands;

public record DeleteApiResourceCommand(Guid Id) : IRequest<bool>;
