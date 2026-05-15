using Microsoft.EntityFrameworkCore;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

    public DbSet<GameEntity> Games { get; set; }
    public DbSet<GachaEntity> GachaStates { get; set; }
    public DbSet<GachaHistoryEntity> GachaHistories { get; set; }
    public DbSet<InventoryEntity> Inventories { get; set; }
    public DbSet<BattleEntity> BattleRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GameEntity>(entity =>
        {
            entity.HasIndex(e => e.Uid)
                  .HasDatabaseName("idx_uid");
        });

        modelBuilder.Entity<GachaEntity>(entity =>
        {
            entity.HasIndex(e => new { e.Uid, e.Index })
                  .IsUnique()
                  .HasDatabaseName("uk_uid_index");

            entity.HasIndex(e => e.Uid)
                  .HasDatabaseName("idx_uid");
        });

        modelBuilder.Entity<GachaHistoryEntity>(entity =>
        {
            entity.HasIndex(e => e.Uid)
                  .HasDatabaseName("idx_uid");
        });

        modelBuilder.Entity<InventoryEntity>(entity =>
        {
            // One row per (user, item) — Count tracks quantity
            entity.HasIndex(e => new { e.Uid, e.ItemId })
                  .IsUnique()
                  .HasDatabaseName("uk_uid_item");

            entity.HasIndex(e => e.Uid)
                  .HasDatabaseName("idx_uid");
        });

        modelBuilder.Entity<BattleEntity>(entity =>
        {
            entity.HasIndex(e => e.Uid)
                  .HasDatabaseName("idx_uid");
        });
    }
}
