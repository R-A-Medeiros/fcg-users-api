using FIAP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FIAP.Domain.Security.Tokens;

public interface IAccessTokenGenerator
{
    string Generate(User user);
}
