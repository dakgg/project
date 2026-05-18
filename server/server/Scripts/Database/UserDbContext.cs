using Microsoft.EntityFrameworkCore;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<UserTokenEntity> UserTokens { get; set; }
}
