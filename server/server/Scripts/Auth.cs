using dakg.shared;

public class AuthHandler()
{
    public async Task<LoginResponse> Login(LoginRequest request)
    {
    
        return new LoginResponse();
        // 간단한 인증 로직 (예: 하드코딩된 사용자명과 비밀번호 확인)

    }
}