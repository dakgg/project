using dakg.shared;
using Microsoft.EntityFrameworkCore;

public class GachaHandler
{
    public const int GemCostPerPull = 100;
    public const int GemCostForTen  = 1000;

    private static int GoldRewardForRarity(string rarity) => rarity switch
    {
        "SSR" => 500,
        "SR"  => 50,
        _     => 10,
    };

    // ThreadLocal avoids locking on a shared Random instance across concurrent requests.
    private static readonly ThreadLocal<Random> _random =
        new(() => new Random(Environment.TickCount + Thread.CurrentThread.ManagedThreadId));

    private static Random Rng => _random.Value!;

    private static readonly Dictionary<int, GachaPool> _pools = new()
    {
        [0] = new GachaPool
        {
            SsrItems = [3001, 3002, 3003],
            SrItems  = [2001, 2002, 2003, 2004, 2005],
            RItems   = [1001, 1002, 1003, 1004, 1005, 1006, 1007, 1008, 1009, 1010],
        }
    };

    private readonly UserDbContext _userDb;
    private readonly GameDbShardManager _shardManager;
    private readonly GameShardTransactionContext _gameShardCtx;

    public GachaHandler(
        UserDbContext userDb,
        GameDbShardManager shardManager,
        GameShardTransactionContext gameShardCtx)
    {
        _userDb = userDb;
        _shardManager = shardManager;
        _gameShardCtx = gameShardCtx;
    }

    public async Task<GachaResponse> Gacha(GachaRequest request)
    {
        var user = await _userDb.Users.FindAsync(request.Uid);
        if (user == null)
            return new GachaResponse { Result = ResponseResult.Error };

        if (request.Count is not (1 or 10))
            return new GachaResponse { Result = ResponseResult.Error };

        var shard = _shardManager.GetShard(user);
        await _gameShardCtx.SetShardAsync(shard);

        var game = await shard.Games.FirstOrDefaultAsync(g => g.Uid == user.Id);
        if (game == null)
        {
            game = new GameEntity { Uid = user.Id, Name = user.Username };
            shard.Games.Add(game);
        }

        int gemCost = request.Count == 10 ? GemCostForTen : GemCostPerPull;
        if (game.Gems < gemCost)
            return new GachaResponse { Result = ResponseResult.Error };

        var state = await shard.GachaStates
            .FirstOrDefaultAsync(g => g.Uid == user.Id && g.Index == request.GachaIndex);

        if (state == null)
        {
            state = new GachaEntity { Uid = user.Id, Index = request.GachaIndex };
            shard.GachaStates.Add(state);
        }

        var items = Roll(state, request.Count);

        game.Gems -= gemCost;
        foreach (var item in items)
        {
            int goldReward = GoldRewardForRarity(item.Rarity);
            game.Gold += goldReward;

            shard.GachaHistories.Add(new GachaHistoryEntity
            {
                Uid        = user.Id,
                PoolIndex  = request.GachaIndex,
                ItemId     = item.ItemId,
                Rarity     = item.Rarity,
                GoldReward = goldReward,
                CreatedAt  = DateTime.UtcNow,
            });

            var slot = await shard.Inventories
                .FirstOrDefaultAsync(i => i.Uid == user.Id && i.ItemId == item.ItemId);

            if (slot != null)
                slot.Count++;
            else
                shard.Inventories.Add(new InventoryEntity { Uid = user.Id, ItemId = item.ItemId });
        }

        return new GachaResponse
        {
            Items     = items,
            PityCount = state.PityCount,
            Gold      = game.Gold,
            Gems      = game.Gems,
        };
    }

    // Rates: SSR 0.6%, SR 5.1%, R 94.3%.
    // Soft pity starts at pull 76 (+6% SSR per pull). Hard pity forces SSR at pull 90.
    private static List<GachaItem> Roll(GachaEntity state, int count)
    {
        if (!_pools.TryGetValue(state.Index, out var pool))
            throw new InvalidOperationException($"Unknown gacha pool index: {state.Index}");

        var results = new List<GachaItem>(count);

        for (int i = 0; i < count; i++)
        {
            state.PityCount++;

            var rarity = DetermineRarity(state.PityCount);

            if (rarity == GachaRarity.SSR)
                state.PityCount = 0;

            state.UpdatedTimeUtc = DateTime.UtcNow;

            results.Add(new GachaItem
            {
                ItemId = PickItem(pool, rarity),
                Rarity = rarity.ToString(),
            });
        }

        return results;
    }

    private static GachaRarity DetermineRarity(int pity)
    {
        if (pity >= 90) return GachaRarity.SSR;

        double ssrRate = pity <= 75 ? 0.006 : 0.006 + (pity - 75) * 0.06;
        ssrRate = Math.Min(ssrRate, 1.0);

        double roll = Rng.NextDouble();

        if (roll < ssrRate) return GachaRarity.SSR;
        if (roll < ssrRate + 0.051) return GachaRarity.SR;
        return GachaRarity.R;
    }

    private static int PickItem(GachaPool pool, GachaRarity rarity)
    {
        var list = rarity switch
        {
            GachaRarity.SSR => pool.SsrItems,
            GachaRarity.SR  => pool.SrItems,
            _               => pool.RItems,
        };
        return list[Rng.Next(list.Length)];
    }

    private enum GachaRarity { R, SR, SSR }

    private sealed class GachaPool
    {
        public int[] SsrItems { get; init; } = [];
        public int[] SrItems  { get; init; } = [];
        public int[] RItems   { get; init; } = [];
    }
}
