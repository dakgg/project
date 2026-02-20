using dakg.shared;
using Microsoft.EntityFrameworkCore;
using Serilog;

public class AuthHandler
{
    private readonly UserDbContext _db;

    public AuthHandler(UserDbContext db)
    {
        _db = db;
    }

    public async Task<LoginResponse> Login(LoginRequest request)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.PublicKey == request.PublicKey && u.PrivateKey == request.PrivateKey);

        if (user == null)
        {
            user = new UserEntity
            {
                PublicKey = request.PublicKey,
                PrivateKey = request.PrivateKey,
            };
            _db.Users.Add(user);
        }
        else
        {
            user.LastLoginTimeUtc = DateTime.UtcNow;
        }

        return new LoginResponse
        {
            User = new User { Id = user.Id }
        };
    }
}