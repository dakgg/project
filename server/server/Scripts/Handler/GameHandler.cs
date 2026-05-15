using dakg.shared;
using Microsoft.EntityFrameworkCore;

public class GameHandler
{
    private readonly UserDbContext _userDb;
    private readonly GameDbShardManager _shardManager;
    private readonly GameShardTransactionContext _gameShardCtx;
    private readonly GachaService _gachaService;

    public GameHandler(
        UserDbContext userDb,
        GameDbShardManager shardManager,
        GameShardTransactionContext gameShardCtx,
        GachaService gachaService)
    {
        _userDb = userDb;
        _shardManager = shardManager;
        _gameShardCtx = gameShardCtx;
        _gachaService = gachaService;
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
            game = new GameEntity { Uid = user.Id, Name = user.PublicKey };
            shard.Games.Add(game);
        }

        int gemCost = request.Count == 10 ? GachaService.GemCostForTen : GachaService.GemCostPerPull;
        if (game.Gems < gemCost)
            return new GachaResponse { Result = ResponseResult.Error };

        var state = await shard.GachaStates
            .FirstOrDefaultAsync(g => g.Uid == user.Id && g.Index == request.GachaIndex);

        if (state == null)
        {
            state = new GachaEntity { Uid = user.Id, Index = request.GachaIndex };
            shard.GachaStates.Add(state);
        }

        var items = _gachaService.Roll(state, request.Count);

        game.Gems -= gemCost;
        foreach (var item in items)
        {
            int goldReward = GachaService.GoldRewardForRarity(item.Rarity);
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
}
