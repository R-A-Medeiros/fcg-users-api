using FIAP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FIAP.Infrastructure.DataAccess;

public class UsersDbContext : DbContext
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Name).IsRequired().HasMaxLength(150);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Password).IsRequired();
            entity.Property(u => u.UserIdentifier).IsRequired();
            entity.HasIndex(u => u.UserIdentifier).IsUnique();
            entity.Property(u => u.Role).IsRequired().HasMaxLength(50);
            entity.Property(u => u.CreatedAt).IsRequired();
        });
    }
}
