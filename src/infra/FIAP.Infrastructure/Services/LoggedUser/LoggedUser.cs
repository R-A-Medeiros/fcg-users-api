using FIAP.Domain.Entities;
using FIAP.Domain.Security.Tokens;
using FIAP.Domain.Services.LoggedUser;
using FIAP.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FIAP.Infrastructure.Services.LoggedUser;

public class LoggedUser : ILoggedUser
{
    private readonly UsersDbContext _dbcontext;
    private readonly ITokenProvider _tokenProvider;

    public LoggedUser(UsersDbContext dbcontext,
                      ITokenProvider tokenProvider)
    {
        _dbcontext = dbcontext;
        _tokenProvider = tokenProvider;
    }
    public async Task<User> Get()
    {
        string token = _tokenProvider.TokenOnRequest();

        var tokenHandler = new JwtSecurityTokenHandler();

        var jwtSecurityToken = tokenHandler.ReadJwtToken(token);

        var identifier = jwtSecurityToken.Claims.First(claim => claim.Type == ClaimTypes.Sid).Value;

        return await _dbcontext.Users
             .AsNoTracking()
             .FirstAsync(user => user.UserIdentifier == Guid.Parse(identifier));
    }
}
