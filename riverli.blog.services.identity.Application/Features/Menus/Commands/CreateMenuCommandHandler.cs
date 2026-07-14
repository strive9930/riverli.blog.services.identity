using MediatR;
using Microsoft.Extensions.Caching.Memory;
using riverli.blog.services.identity.Application.Constants;
using riverli.blog.services.identity.Domain.Entities;
using riverli.blog.services.identity.Domain.Enums;
using RiverLi.DDD.Core.Domain.Repositories;

namespace riverli.blog.services.identity.Application.Features.Menus.Commands;

public class CreateMenuCommandHandler : IRequestHandler<CreateMenuCommand, bool>
{
    private readonly IRepository<Menu, Guid> _repository;
    private readonly IMemoryCache _cache;

    public CreateMenuCommandHandler(IRepository<Menu, Guid> repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<bool> Handle(CreateMenuCommand request, CancellationToken cancellationToken)
    {
        var menu = new Menu
        {
            Id = Guid.NewGuid(),
            Name = request.Data.Name,
            Title = request.Data.Title,
            Path = request.Data.Path,
            Component = request.Data.Component,
            Icon = request.Data.Icon,
            ParentId = request.Data.ParentId,
            SortOrder = request.Data.SortOrder,
            Type = request.Data.Type
        };
        await _repository.AddAsync(menu, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        _cache.Remove(CacheKeys.SysMenuTree);
        return true;
    }
}