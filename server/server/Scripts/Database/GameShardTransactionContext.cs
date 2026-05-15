using Microsoft.EntityFrameworkCore.Storage;

/// <summary>
/// Scoped service that lazily enlists a single game DB shard into the current request's transaction.
/// Handlers call <see cref="SetShardAsync"/> to register the shard they need;
/// <see cref="TransactionMiddleware"/> calls <see cref="CommitAsync"/> or <see cref="RollbackAsync"/>.
/// Only one shard per request is supported—matching the sharding design where each user maps to one shard.
/// </summary>
public class GameShardTransactionContext : IAsyncDisposable
{
    private GameDbContext? _shard;
    private IDbContextTransaction? _transaction;

    public GameDbContext? Shard => _shard;

    public async Task SetShardAsync(GameDbContext shard)
    {
        if (_shard != null)
            throw new InvalidOperationException("Only one game shard may be enlisted per request.");

        _shard = shard;
        _transaction = await shard.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        if (_shard == null || _transaction == null) return;

        await _shard.SaveChangesAsync();
        await _transaction.CommitAsync();
    }

    public async Task RollbackAsync()
    {
        if (_transaction == null) return;
        await _transaction.RollbackAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction != null)
            await _transaction.DisposeAsync();
    }
}
