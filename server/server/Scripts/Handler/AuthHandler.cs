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

            // DB가 생성한 Id를 응답에 쓰기 위해 여기서 SaveChanges 호출.
            // EF Core가 열린 트랜잭션 안에서 SQL을 flush하고, 커밋은 미들웨어가 처리함.
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