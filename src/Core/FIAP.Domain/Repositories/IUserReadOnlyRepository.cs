namespace FIAP.Domain.Repositories;

public interface IUserReadOnlyRepository
{
    Task<bool> ExistActiveUserWithEmail(string email);

    Task<Entities.User?> GetUserByEmail(string email);

    Task<Entities.User?> GetUserByIdentifier(Guid userIdentifier);

    Task<bool> ExistUserWithRole(string role);
}
