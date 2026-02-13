namespace dakg.shared
{
    public class RequestBase(int messageId)
    {
        public MessageId MessageId { get; set; } = (MessageId)messageId;
    }

    public class ResponseBase(int messageId)
    {
        public MessageId MessageId { get; set; } = (MessageId)messageId;
    }
}