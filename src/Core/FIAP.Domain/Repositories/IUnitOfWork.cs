namespace FIAP.Domain.Repositories;

public interface IUnitOfWork
{
    Task Commit();
}
