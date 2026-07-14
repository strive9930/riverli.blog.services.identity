using MediatR;
using Microsoft.Extensions.Caching.Memory;
using riverli.blog.services.identity.Application.Constants;
using riverli.blog.services.identity.Domain.Entities;
using RiverLi.DDD.Core.Domain.Repositories;

namespace riverli.blog.services.identity.Application.Features.ApiResources.Commands;

public class DeleteApiResourceCommandHandler : IRequestHandler<DeleteApiResourceCommand, bool>
{
    private readonly IRepository<ApiResource, Guid> _repository;
    private readonly IMemoryCache _cache;

    public DeleteApiResourceCommandHandler(IRepository<ApiResource, Guid> repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<bool> Handle(DeleteApiResourceCommand request, CancellationToken cancellationToken)
    {
        var apiResource = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (apiResource == null) throw new KeyNotFoundException("API资源不存在");

        // 级联删除：EF Core 会通过非空外键自动清理关联的 RoleApi 记录
        await _repository.DeleteAsync(apiResource, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        _cache.Remove(CacheKeys.SysApiResources);
        _cache.Remove(CacheKeys.SysApiTree);
        return true;
    }
}
