using MediatR;
using Microsoft.Extensions.Caching.Memory;
using riverli.blog.services.identity.Application.Constants;
using riverli.blog.services.identity.Domain.Entities;
using riverli.blog.services.identity.Domain.Enums;
using RiverLi.DDD.Core.Domain.Repositories;

namespace riverli.blog.services.identity.Application.Features.Menus.Commands;

public class UpdateMenuCommandHandler : IRequestHandler<UpdateMenuCommand, bool>
{
    private readonly IRepository<Menu, Guid> _repository;
    private readonly IMemoryCache _cache;

    public UpdateMenuCommandHandler(IRepository<Menu, Guid> repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<bool> Handle(UpdateMenuCommand request, CancellationToken cancellationToken)
    {
        var menu = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (menu == null) throw new KeyNotFoundException("菜单不存在");

        menu.Name = request.Data.Name;
        menu.Title = request.Data.Title;
        menu.Path = request.Data.Path;
        menu.Component = request.Data.Component;
        menu.Icon = request.Data.Icon;
        menu.ParentId = request.Data.ParentId;
        menu.SortOrder = request.Data.SortOrder;
        menu.Type = request.Data.Type;

        await _repository.UpdateAsync(menu, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        _cache.Remove(CacheKeys.SysMenuTree);
        return true;
    }
}