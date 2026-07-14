using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using riverli.blog.services.identity.Application.Interfaces;
using riverli.blog.services.identity.Domain.Entities;

namespace riverli.blog.services.identity.Infrastructure.Auth;

public class JwtProvider : IJwtProvider
{
    private readonly IConfiguration _configuration;

    public JwtProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(AppUser user, IList<string> roles, IList<string> permissions)
    {
        // 1. 读取 "Jwt" 节点
        var jwtSettings = _configuration.GetSection("Jwt");
        
        // 2. 读取 SecretKey
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        
        foreach (var perm in permissions)
        {
            if (!string.IsNullOrWhiteSpace(perm))
            {
                claims.Add(new Claim("perm", perm)); 
            }
        }
        
        // 3. 读取 ExpiryMinutes，如果没配则默认给 60 分钟
        var expiryMinutesStr = jwtSettings["ExpiryMinutes"];
        var expiryMinutes = int.TryParse(expiryMinutesStr, out var parsed) ? parsed : 60;

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes), // 使用动态过期时间
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}