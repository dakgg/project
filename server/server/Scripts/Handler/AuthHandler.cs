using dakg.shared;
using Microsoft.EntityFrameworkCore;
using Serilog;

public class AuthHandler
{
    private readonly AppDbContext _db;

    public AuthHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<LoginResponse> Login(LoginRequest request)
    {
        Log.Information("LoginRequest received");

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.PublicKey == request.PublicKey && u.PrivateKey == request.PrivateKey);

        if (user == null)
        {
            throw new UnauthorizedAccessException();
        }

        return new LoginResponse();
    }
}