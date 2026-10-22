using FIAP.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace FIAP.Domain.Entities;

public class User
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Guid UserIdentifier { get; set; }
    public string Role { get; set; } = Roles.USER;

    public DateTime CreatedAt { get; set; }
}
