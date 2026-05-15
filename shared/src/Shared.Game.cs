namespace dakg.shared
{
    public class GachaRequest : RequestBase
    {
        public GachaRequest() : base((int)MessageId.GACHA_REQUEST)
        {
        }

        public string Message { get; set; }
    }

    public class GachaResponse : ResponseBase
    {
        public GachaResponse() : base((int)ResponseResult.SUCCESS)
        {
        }

        public string Message { get; set; }
    }
}