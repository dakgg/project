using System.Security.Cryptography;
using dakg.shared;
using Microsoft.EntityFrameworkCore;

public class AuthHandler
{
    private static readonly TimeSpan TokenExpiry = TimeSpan.FromDays(7);

    private readonly UserDbContext _db;

    public AuthHandler(UserDbContext db)
    {
        _db = db;
    }

    public async Task<RegisterResponse> Register(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return new RegisterResponse { Result = ResponseResult.Error };

        bool exists = await _db.Users.AnyAsync(u => u.Username == request.Username);
        if (exists)
            return new RegisterResponse { Result = ResponseResult.Error };

        var (hash, salt) = HashPassword(request.Password);

        var user = new UserEntity
        {
            Username = request.Username,
            PasswordHash = hash,
            Salt = salt,
        };
        _db.Users.Add(user);

        // DB가 생성한 Id를 토큰에 쓰기 위해 여기서 SaveChanges 호출.
        // EF Core가 열린 트랜잭션 안에서 SQL을 flush하고, 커밋은 미들웨어가 처리함.
        await _db.SaveChangesAsync();

        var token = await CreateTokenAsync(user.Id);
        return new RegisterResponse { Uid = user.Id, Token = token };
    }

    public async Task<LoginResponse> Login(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user == null)
            return new LoginResponse { Result = ResponseResult.Error };

        if (!VerifyPassword(request.Password, user.PasswordHash, user.Salt))
            return new LoginResponse { Result = ResponseResult.Error };

        user.LastLoginTimeUtc = DateTime.UtcNow;

        var token = await CreateTokenAsync(user.Id);
        return new LoginResponse { Uid = user.Id, Token = token };
    }

    public async Task<LogoutResponse> Logout(LogoutRequest request)
    {
        var tokenEntity = await _db.UserTokens.FirstOrDefaultAsync(t => t.Token == request.Token);
        if (tokenEntity != null)
            _db.UserTokens.Remove(tokenEntity);

        return new LogoutResponse();
    }

    // --- 비밀번호 해싱 (PBKDF2 / HMAC-SHA256) ---

    private const int Iterations = 10_000;
    private const int SaltSize   = 16; // 128 bit
    private const int KeySize    = 32; // 256 bit

    private static (string hash, string salt) HashPassword(string password)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            password, saltBytes, Iterations, HashAlgorithmName.SHA256, KeySize);
        return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
    }

    private static bool VerifyPassword(string password, string hash, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            password, saltBytes, Iterations, HashAlgorithmName.SHA256, KeySize);
        return Convert.ToBase64String(hashBytes) == hash;
    }

    // --- 토큰 생성 ---

    private async Task<string> CreateTokenAsync(long uid)
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(16);
        var token = Convert.ToBase64String(tokenBytes)
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');

        _db.UserTokens.Add(new UserTokenEntity
        {
            Token     = token,
            Uid       = uid,
            ExpiresAt = DateTime.UtcNow.Add(TokenExpiry),
        });

        return token;
    }
}
