using MediatR;
using Microsoft.Extensions.Caching.Memory;
using riverli.blog.services.identity.Application.Constants;
using riverli.blog.services.identity.Domain.Entities;
using RiverLi.DDD.Core.Domain.Repositories;

namespace riverli.blog.services.identity.Application.Features.ApiResources.Commands;

public class UpdateApiResourceCommandHandler : IRequestHandler<UpdateApiResourceCommand, bool>
{
    private readonly IRepository<ApiResource, Guid> _repository;
    private readonly IMemoryCache _cache;

    public UpdateApiResourceCommandHandler(IRepository<ApiResource, Guid> repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<bool> Handle(UpdateApiResourceCommand request, CancellationToken cancellationToken)
    {
        var apiResource = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (apiResource == null) throw new KeyNotFoundException("API资源不存在");

        apiResource.ServiceName = request.Data.ServiceName;
        apiResource.Method = request.Data.Method;
        apiResource.Route = request.Data.Route;
        apiResource.Description = request.Data.Description;
        apiResource.IsPublic = request.Data.IsPublic;

        await _repository.UpdateAsync(apiResource, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        _cache.Remove(CacheKeys.SysApiResources);
        _cache.Remove(CacheKeys.SysApiTree);
        return true;
    }
}
