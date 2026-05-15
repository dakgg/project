using dakg.shared;

/// <summary>
/// Handles gacha roll logic with soft and hard pity.
/// Rates: SSR 0.6%, SR 5.1%, R 94.3%.
/// Soft pity starts at pull 76 (+6% SSR per pull).
/// Hard pity forces SSR at pull 90 and resets the counter.
/// </summary>
public class GachaService
{
    public const int GemCostPerPull = 100;
    public const int GemCostForTen  = 1000;

    public static int GoldRewardForRarity(string rarity) => rarity switch
    {
        "SSR" => 500,
        "SR"  => 50,
        _     => 10,
    };

    // ThreadLocal avoids locking on a shared Random instance across concurrent requests.
    private static readonly ThreadLocal<Random> _random =
        new(() => new Random(Environment.TickCount + Thread.CurrentThread.ManagedThreadId));

    private static Random Rng => _random.Value!;

    // Item table per pool index. Replace with TableManager when data pipeline is ready.
    private static readonly Dictionary<int, GachaPool> _pools = new()
    {
        [0] = new GachaPool
        {
            SsrItems = [3001, 3002, 3003],
            SrItems  = [2001, 2002, 2003, 2004, 2005],
            RItems   = [1001, 1002, 1003, 1004, 1005, 1006, 1007, 1008, 1009, 1010],
        }
    };

    public List<GachaItem> Roll(GachaEntity state, int count)
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
        // Hard pity: guaranteed SSR at pull 90
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
