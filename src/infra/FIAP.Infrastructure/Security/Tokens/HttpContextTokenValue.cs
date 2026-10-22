using FIAP.Domain.Security.Tokens;
using Microsoft.AspNetCore.Http;

namespace FIAP.Infrastructure.Security.Tokens;

public class HttpContextTokenValue : ITokenProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public HttpContextTokenValue(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    public string TokenOnRequest()
    {
        //var authorization = _httpContextAccessor.HttpContext!.Request.Headers.Authorization.ToString();

        var authorization = _httpContextAccessor.HttpContext!.Request.Headers["Authorization"].ToString();

        return authorization["Bearer ".Length..].Trim();

        //return authorization.Replace("Bearer ", string.Empty);
    }
}
