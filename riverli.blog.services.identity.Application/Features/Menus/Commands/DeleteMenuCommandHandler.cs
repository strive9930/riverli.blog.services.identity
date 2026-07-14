using MediatR;
using Microsoft.Extensions.Caching.Memory;
using riverli.blog.services.identity.Application.Constants;
using riverli.blog.services.identity.Domain.Entities;
using RiverLi.DDD.Core.Domain.Repositories;
using System.Linq.Expressions;

namespace riverli.blog.services.identity.Application.Features.Menus.Commands;

public class DeleteMenuCommandHandler : IRequestHandler<DeleteMenuCommand, bool>
{
    private readonly IRepository<Menu, Guid> _repository;
    private readonly IMemoryCache _cache;

    public DeleteMenuCommandHandler(IRepository<Menu, Guid> repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<bool> Handle(DeleteMenuCommand request, CancellationToken cancellationToken)
    {
        // 核心防御：检查是否有子菜单
        var hasChildren = await _repository.ExistsAsync(m => m.ParentId == request.Id, cancellationToken);
        if (hasChildren) throw new InvalidOperationException("该节点下存在子菜单，请先删除子菜单");

        var menu = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (menu == null) throw new KeyNotFoundException("菜单不存在");

        await _repository.DeleteAsync(menu, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        _cache.Remove(CacheKeys.SysMenuTree);
        return true;
    }
}