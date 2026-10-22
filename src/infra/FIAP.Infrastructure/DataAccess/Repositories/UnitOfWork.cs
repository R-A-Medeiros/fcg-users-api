using FIAP.Domain.Repositories;

namespace FIAP.Infrastructure.DataAccess.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly UsersDbContext _context;
    public UnitOfWork(UsersDbContext context)
    {
        _context = context;
    }

    public async Task Commit()
    {
        await _context.SaveChangesAsync();
    }
}
