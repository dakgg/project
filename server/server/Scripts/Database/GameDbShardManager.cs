using Microsoft.EntityFrameworkCore;

public class GameDbShardConfig
{
    public List<(string ConnectionString, ServerVersion ServerVersion)> Shards { get; set; } = [];
}

public class GameDbShardManager : IAsyncDisposable
{
    private readonly List<GameDbContext> _contexts;

    public int ShardCount => _contexts.Count;

    public GameDbShardManager(GameDbShardConfig config)
    {
        _contexts = config.Shards.Select(shard =>
        {
            var options = new DbContextOptionsBuilder<GameDbContext>()
                .UseMySql(shard.ConnectionString, shard.ServerVersion)
                .Options;
            return new GameDbContext(options);
        }).ToList();
    }

    public GameDbContext GetShard(UserEntity user) => _contexts[(int)(user.Id % _contexts.Count)];

    public IReadOnlyList<GameDbContext> All => _contexts;

    public async ValueTask DisposeAsync()
    {
        foreach (var ctx in _contexts)
            await ctx.DisposeAsync();
    }
}
