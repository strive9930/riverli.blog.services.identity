using MediatR;
using riverli.blog.services.identity.Application.DTOs.ApiResources;

namespace riverli.blog.services.identity.Application.Features.ApiResources.Commands;

public record CreateApiResourceCommand(CreateOrUpdateApiResourceDto Data) : IRequest<bool>;
