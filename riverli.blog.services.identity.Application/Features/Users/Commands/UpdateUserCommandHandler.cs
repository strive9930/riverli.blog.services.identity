using MediatR;
using riverli.blog.services.identity.Domain.Entities;
using RiverLi.DDD.Core.Domain.Repositories;

namespace riverli.blog.services.identity.Application.Features.Users.Commands;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, bool>
{
    private readonly IRepository<AppUser, Guid> _repository;
    public UpdateUserCommandHandler(IRepository<AppUser, Guid> repository) => _repository = repository;

    public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (user == null) throw new KeyNotFoundException("用户不存在");

        user.RealName = request.Data.RealName;
        user.Email = request.Data.Email;
        user.IsActive = request.Data.IsActive;

        await _repository.UpdateAsync(user, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return true;
    }
}
