using FIAP.Domain.Entities;
using FIAP.Domain.Security.Tokens;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FIAP.Infrastructure.Security.Tokens;

public class JwtTokenGenerator : IAccessTokenGenerator
{
    private readonly uint _expirationTimeMinutes;
    private readonly string _signingKey;
    private readonly string? _issuer;
    private readonly string? _audience;

    public JwtTokenGenerator(uint expirationTimeMinutes, string signinKey, string? issuer = null, string? audience = null)
    {
        _expirationTimeMinutes = expirationTimeMinutes;
        _signingKey = signinKey;
        _issuer = issuer;
        _audience = audience;
    }

    public string Generate(User user)
    {
        var claims = new List<Claim>()
        {
            // NameIdentifier/sub carries the user identifier so other services (CatalogAPI)
            // can read it as the UserId.
            new Claim(ClaimTypes.NameIdentifier, user.UserIdentifier.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Sid, user.UserIdentifier.ToString()),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _issuer,
            Audience = _audience,
            Expires = DateTime.UtcNow.AddMinutes(_expirationTimeMinutes),
            SigningCredentials = new SigningCredentials(SecurityKey(), SecurityAlgorithms.HmacSha256Signature),
            Subject = new ClaimsIdentity(claims)
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var securityToken = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(securityToken);
    }

    private SymmetricSecurityKey SecurityKey()
    {
        var key = Encoding.UTF8.GetBytes(_signingKey);
        return new SymmetricSecurityKey(key);
    }
}
