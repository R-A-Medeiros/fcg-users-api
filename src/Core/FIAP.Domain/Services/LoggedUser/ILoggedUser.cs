using FIAP.Domain.Entities;

namespace FIAP.Domain.Services.LoggedUser;

public interface ILoggedUser
{
    Task<User> Get();
}
