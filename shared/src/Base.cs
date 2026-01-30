namespace dakg.shared
{
    public class RequestBase
    {
        public MessageId MessageId;

        public RequestBase(int messageId)
        {
            MessageId = (MessageId)messageId;
        }
    }
}