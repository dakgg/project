namespace dakg.shared
{
    public class BattleRequest : RequestBase
    {
        public BattleRequest() : base((int)MessageId.BATTLE_REQUEST) { }

        public long Uid { get; set; }
        public int StageId { get; set; }
    }

    public class BattleResponse : ResponseBase
    {
        public BattleResponse() : base((int)ResponseResult.SUCCESS) { }

        public bool IsWin { get; set; }
        public int RewardGold { get; set; }
        public long Gold { get; set; }
    }
}
