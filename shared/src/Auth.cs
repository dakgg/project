namespace dakg.shared
{
    public class LoginRequest : RequestBase
    {
        public LoginRequest() : base((int)MessageId.LOGIN_REQUEST)
        {
        }
    }

    public class LoginResponse
    {
    }
}