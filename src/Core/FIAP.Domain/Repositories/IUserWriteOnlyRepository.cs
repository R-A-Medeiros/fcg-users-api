namespace FIAP.Domain.Repositories;

public interface IUserWriteOnlyRepository
{
    Task Add(Entities.User user);

    void Update(Entities.User user);
}
