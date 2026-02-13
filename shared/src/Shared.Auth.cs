namespace dakg.shared
{
    public class LoginRequest : RequestBase
    {
        public LoginRequest() : base((int)MessageId.LOGIN_REQUEST)
        {
        }

        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }

    }

    public class LoginResponse : ResponseBase
    {
        public LoginResponse() : base((int)ResponseResult.SUCCESS)
        {
        }
    }
}