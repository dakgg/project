// AppDbContext는 UserDbContext, GameDb1Context, GameDb2Context로 분리되었습니다.
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<GameEntity> Games { get; set; }
}