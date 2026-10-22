using System;
using System.Collections.Generic;
using System.Text;

namespace FIAp.Exception.ExceptionsBase;

public abstract class UsersAPIException : SystemException
{
    protected UsersAPIException(string message) : base(message)
    {

    }

    public abstract int StatusCode { get; }
    public abstract List<string> GetErrors();
}
