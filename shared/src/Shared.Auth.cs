namespace dakg.shared
{
    public class LoginRequest : RequestBase
    {
        public LoginRequest() : base((int)MessageId.LOGIN_REQUEST) { }

        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse : ResponseBase
    {
        public LoginResponse() : base((int)ResponseResult.SUCCESS) { }

        public long Uid { get; set; }
        public string Token { get; set; } = string.Empty;
    }

    public class RegisterRequest : RequestBase
    {
        public RegisterRequest() : base((int)MessageId.REGISTER_REQUEST) { }

        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterResponse : ResponseBase
    {
        public RegisterResponse() : base((int)ResponseResult.SUCCESS) { }

        public long Uid { get; set; }
        public string Token { get; set; } = string.Empty;
    }

    public class LogoutRequest : RequestBase
    {
        public LogoutRequest() : base((int)MessageId.LOGOUT_REQUEST) { }
    }

    public class LogoutResponse : ResponseBase
    {
        public LogoutResponse() : base((int)ResponseResult.SUCCESS) { }
    }
}
