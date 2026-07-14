using riverli.blog.services.identity.Domain.Entities;

namespace riverli.blog.services.identity.Application.Interfaces;

public interface IJwtProvider
{
    string GenerateToken(AppUser user, IList<string> roles, IList<string> permissions);
}