
namespace dakg.shared
{
    public class TestRequest : RequestBase
    {
        public TestRequest() : base((int)MessageId.TEST_REQUEST)
        {
        }

        public string TestData { get; set; }
    }

    public class TestResponse : ResponseBase
    {
        public TestResponse() : base((int)ResponseResult.SUCCESS)
        {
        }

        public string ResponseData { get; set; }
    }
}