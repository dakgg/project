using dakg.shared;
using Microsoft.EntityFrameworkCore;

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
            .FirstOrDefaultAsync(u => u.PublicKey == request.PublicKey);

        if (user == null)
        {
            user = new UserEntity
            {
                PublicKey = request.PublicKey,
                PrivateKey = request.PrivateKey,
            };
            _db.Users.Add(user);

            // SaveChanges here so the DB-generated Id is available before building the response.
            // EF Core flushes SQL within the open transaction; the middleware commits it afterwards.
            await _db.SaveChangesAsync();
        }
        else if (user.PrivateKey != request.PrivateKey)
        {
            return new LoginResponse { Result = ResponseResult.Error };
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