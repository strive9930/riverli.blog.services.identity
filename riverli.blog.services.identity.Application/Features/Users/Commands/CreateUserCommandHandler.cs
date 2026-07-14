using MediatR;
using Microsoft.AspNetCore.Identity;
using riverli.blog.services.identity.Domain.Entities;
using RiverLi.DDD.Core.Domain.Repositories;

namespace riverli.blog.services.identity.Application.Features.Users.Commands;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, bool>
{
    private readonly IRepository<AppUser, Guid> _repository;
    private readonly IPasswordHasher<AppUser> _passwordHasher;
    public CreateUserCommandHandler(IRepository<AppUser, Guid> repository, IPasswordHasher<AppUser> passwordHasher)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
    }

    public async Task<bool> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new AppUser { Id = Guid.NewGuid(), UserName = request.Data.Username, RealName = request.Data.RealName, Email = request.Data.Email, SecurityStamp = Guid.NewGuid().ToString() };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Data.Password);
        await _repository.AddAsync(user, cancellationToken);
        await _repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return true;
    }
}