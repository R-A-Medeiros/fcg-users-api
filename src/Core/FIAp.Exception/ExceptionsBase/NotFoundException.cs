using System.Net;

namespace FIAp.Exception.ExceptionsBase;

public class NotFoundException : UsersAPIException
{
    public NotFoundException(string message) : base(message)
    {
    }

    public override int StatusCode => (int)HttpStatusCode.NotFound;

    public override List<string> GetErrors()
    {
        return [Message];
    }
}
