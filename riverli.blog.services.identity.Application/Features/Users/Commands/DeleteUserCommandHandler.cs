using MediatR;
using riverli.blog.services.identity.Domain.Entities;
using RiverLi.DDD.Core.Domain.Repositories;

namespace riverli.blog.services.identity.Application.Features.Users.Commands;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
{
    private readonly IRepository<AppUser, Guid> _repository;

    public DeleteUserCommandHandler(IRepository<AppUser, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException("未找到要删除的用户");
        }

        // 超级管理员账号不允许删除 (安全防御)
        if (user.UserName == "admin")
        {
            throw new InvalidOperationException("内置超级管理员账号禁止删除");
        }
        // 逻辑删除
        user.IsActive = false;
        await _repository.UpdateAsync(user, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return true;
    }
}