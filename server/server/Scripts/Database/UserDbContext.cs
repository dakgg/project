using Microsoft.EntityFrameworkCore;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

    public DbSet<UserEntity> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.HasIndex(e => e.PublicKey)
                  .HasDatabaseName("idx_public_key");
        });
    }
}
