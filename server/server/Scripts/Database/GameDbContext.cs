using Microsoft.EntityFrameworkCore;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

    public DbSet<GameEntity> Games { get; set; }
    public DbSet<GachaEntity> GachaStates { get; set; }
    public DbSet<GachaHistoryEntity> GachaHistories { get; set; }
    public DbSet<InventoryEntity> Inventories { get; set; }
    public DbSet<BattleEntity> BattleRecords { get; set; }
}
