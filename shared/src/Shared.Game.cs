namespace dakg.shared
{
    public class GachaRequest : RequestBase
    {
        public GachaRequest() : base((int)MessageId.GACHA_REQUEST) { }

        public long Uid { get; set; }           // 유저 ID
        public int GachaIndex { get; set; }     // 가챠 풀 인덱스 (배너)
        public int Count { get; set; } = 1;     // 뽑기 횟수 (1 or 10)
    }

    public class GachaResponse : ResponseBase
    {
        public GachaResponse() : base((int)ResponseResult.SUCCESS) { }

        public List<GachaItem> Items { get; set; } = [];
        public int PityCount { get; set; }
        public long Gold { get; set; }
        public int Gems { get; set; }
    }

    public class GachaItem
    {
        public int ItemId { get; set; }
        public string Rarity { get; set; } = string.Empty;
    }
}