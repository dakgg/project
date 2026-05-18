using dakg.shared;
using Microsoft.EntityFrameworkCore;

public class BattleHandler
{
    private const int AtkPerLevel  = 10;
    private const int HpPerStage   = 50;
    private const int GoldPerStage = 20;

    private readonly UserDbContext _userDb;
    private readonly GameDbShardManager _shardManager;
    private readonly GameShardTransactionContext _gameShardCtx;

    public BattleHandler(
        UserDbContext userDb,
        GameDbShardManager shardManager,
        GameShardTransactionContext gameShardCtx)
    {
        _userDb = userDb;
        _shardManager = shardManager;
        _gameShardCtx = gameShardCtx;
    }

    public async Task<BattleResponse> Battle(BattleRequest request)
    {
        var user = await _userDb.Users.FindAsync(request.Uid);
        if (user == null)
            return new BattleResponse { Result = ResponseResult.Error };

        if (request.StageId <= 0)
            return new BattleResponse { Result = ResponseResult.Error };

        var shard = _shardManager.GetShard(user);
        await _gameShardCtx.SetShardAsync(shard);

        var game = await shard.Games.FirstOrDefaultAsync(g => g.Uid == user.Id);
        if (game == null)
        {
            game = new GameEntity { Uid = user.Id, Name = user.PublicKey };
            shard.Games.Add(game);
        }

        int playerAtk = game.Level * AtkPerLevel;
        int enemyHp   = request.StageId * HpPerStage;
        bool isWin    = playerAtk >= enemyHp;
        int reward    = isWin ? request.StageId * GoldPerStage : 0;

        if (isWin)
            game.Gold += reward;

        shard.BattleRecords.Add(new BattleEntity
        {
            Uid        = user.Id,
            StageId    = request.StageId,
            IsWin      = isWin,
            RewardGold = reward,
            CreatedAt  = DateTime.UtcNow,
        });

        return new BattleResponse
        {
            IsWin      = isWin,
            RewardGold = reward,
            Gold       = game.Gold,
        };
    }
}
