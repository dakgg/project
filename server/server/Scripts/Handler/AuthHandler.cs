using dakg.shared;
using Serilog;

public class AuthHandler
{
    public LoginResponse Login(LoginRequest request)
    {
        Log.Information("LoginRequest received");
        if (request.PublicKey == "admin" && request.PrivateKey == "password")
        {
            return new LoginResponse();
        }
        else
        {
            throw new UnauthorizedAccessException();
        }
    }
}