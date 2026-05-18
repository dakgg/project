namespace dakg.shared
{
    public class RequestBase(int messageId)
    {
        public MessageId MessageId { get; set; } = (MessageId)messageId;
        public string Token { get; set; } = string.Empty;
    }

    public class ResponseBase(int messageId)
    {
        public MessageId MessageId { get; set; } = (MessageId)messageId;
        public ResponseResult Result { get; set; } = ResponseResult.NONE;
    }
}