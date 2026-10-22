namespace FIAP.Domain.Security.Tokens;

public interface ITokenProvider
{
    string TokenOnRequest();
}
