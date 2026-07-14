using MediatR;
using Microsoft.Extensions.Caching.Memory;
using riverli.blog.services.identity.Application.Constants;
using riverli.blog.services.identity.Domain.Entities;
using RiverLi.DDD.Core.Domain.Repositories;

namespace riverli.blog.services.identity.Application.Features.ApiResources.Commands;

public class CreateApiResourceCommandHandler : IRequestHandler<CreateApiResourceCommand, bool>
{
    private readonly IRepository<ApiResource, Guid> _repository;
    private readonly IMemoryCache _cache;

    public CreateApiResourceCommandHandler(IRepository<ApiResource, Guid> repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<bool> Handle(CreateApiResourceCommand request, CancellationToken cancellationToken)
    {
        var apiResource = new ApiResource
        {
            Id = Guid.NewGuid(),
            ServiceName = request.Data.ServiceName,
            Method = request.Data.Method,
            Route = request.Data.Route,
            Description = request.Data.Description,
            IsPublic = request.Data.IsPublic
        };
        await _repository.AddAsync(apiResource, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        _cache.Remove(CacheKeys.SysApiResources);
        _cache.Remove(CacheKeys.SysApiTree);
        return true;
    }
}
