using StackExchange.Redis;
using System.Text.Json;

/// <summary>
/// Thin wrapper around <see cref="IDatabase"/> with JSON serialization.
/// Use for data that is read frequently but changes infrequently (item master, ranking top-N, etc.).
/// Call <see cref="InvalidateUserAsync"/> or <see cref="DeleteAsync"/> whenever the cached value changes.
/// </summary>
public class RedisClient
{
    private readonly IDatabase _db;

    public RedisClient(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(value);
        if (expiry.HasValue)
            await _db.StringSetAsync(key, json, expiry.Value);
        else
            await _db.StringSetAsync(key, json);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(key);
        if (value.IsNullOrEmpty) return default;
        return JsonSerializer.Deserialize<T>(value!);
    }

    public async Task<bool> DeleteAsync(string key) =>
        await _db.KeyDeleteAsync(key);

    public async Task<bool> ExistsAsync(string key) =>
        await _db.KeyExistsAsync(key);

    public Task InvalidateUserAsync(long uid) =>
        DeleteAsync(CacheKey.User(uid));

    public static class CacheKey
    {
        public static string User(long uid) => $"user:{uid}";
        public static string Ranking(int page) => $"ranking:p{page}";
        public static string GachaPool(int index) => $"gacha:pool:{index}";
    }
}
