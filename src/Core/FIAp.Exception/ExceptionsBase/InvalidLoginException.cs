using System.Net;

namespace FIAp.Exception.ExceptionsBase;

public class InvalidLoginException : UsersAPIException
{
    public InvalidLoginException() : base("Email or password invalid.")
    {
    }

    public override int StatusCode => (int)HttpStatusCode.Unauthorized;

    public override List<string> GetErrors()
    {
        return [Message];
    }
}
