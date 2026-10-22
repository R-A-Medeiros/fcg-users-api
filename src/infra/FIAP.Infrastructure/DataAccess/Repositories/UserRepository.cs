using FIAP.Domain.Entities;
using FIAP.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FIAP.Infrastructure.DataAccess.Repositories;

public class UserRepository : IUserReadOnlyRepository, IUserWriteOnlyRepository
{

    private readonly UsersDbContext _context;

    public UserRepository(UsersDbContext context)
    {
        _context = context;
    }

    public async Task Add(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public void Update(User user)
    {
        _context.Users.Update(user);
    }

    public async Task<bool> ExistActiveUserWithEmail(string email)
    {
        return await _context.Users.AnyAsync(user => user.Email.Equals(email));
    }

    public async Task<bool> ExistUserWithRole(string role)
    {
        return await _context.Users.AnyAsync(user => user.Role.Equals(role));
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Email.Equals(email));
    }

    public async Task<User?> GetUserByIdentifier(Guid userIdentifier)
    {
        return await _context.Users
            .FirstOrDefaultAsync(user => user.UserIdentifier.Equals(userIdentifier));
    }
}
