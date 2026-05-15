using dakg.shared;
using Microsoft.EntityFrameworkCore;

public class InventoryHandler
{
    private readonly UserDbContext _userDb;
    private readonly GameDbShardManager _shardManager;
    private readonly GameShardTransactionContext _gameShardCtx;

    public InventoryHandler(
        UserDbContext userDb,
        GameDbShardManager shardManager,
        GameShardTransactionContext gameShardCtx)
    {
        _userDb = userDb;
        _shardManager = shardManager;
        _gameShardCtx = gameShardCtx;
    }

    public async Task<GetInventoryResponse> GetInventory(GetInventoryRequest request)
    {
        var user = await _userDb.Users.FindAsync(request.Uid);
        if (user == null)
            return new GetInventoryResponse { Result = ResponseResult.Error };

        var shard = _shardManager.GetShard(user);
        await _gameShardCtx.SetShardAsync(shard);

        var rows = await shard.Inventories
            .Where(i => i.Uid == user.Id)
            .ToListAsync();

        return new GetInventoryResponse
        {
            Items = rows.Select(r => new InventoryItem
            {
                ItemId = r.ItemId,
                Count  = r.Count,
            }).ToList(),
        };
    }
}
