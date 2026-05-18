using Microsoft.EntityFrameworkCore;

/// <summary>
/// 토큰 검증 미들웨어. TransactionMiddleware 앞에 등록해야 함.
/// LoginRequest, RegisterRequest 경로는 인증 없이 통과.
/// 그 외 모든 요청은 RequestBase.Token이 유효해야 함.
/// </summary>
public class AuthMiddleware
{
    private static readonly HashSet<string> ExcludedPaths =
    [
        "/LoginRequest",
        "/RegisterRequest",
    ];

    private readonly RequestDelegate _next;

    public AuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, UserDbContext userDb)
    {
        if (ExcludedPaths.Contains(context.Request.Path.Value ?? string.Empty))
        {
            await _next(context);
            return;
        }

        string? token = null;

        // 1순위: Authorization: Bearer {token} 헤더
        if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var header = authHeader.FirstOrDefault();
            if (header != null && header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                token = header["Bearer ".Length..].Trim();
        }

        // 2순위: 요청 바디의 Token 필드
        if (string.IsNullOrEmpty(token))
        {
            context.Request.EnableBuffering();

            try
            {
                using var doc = await System.Text.Json.JsonDocument.ParseAsync(context.Request.Body);
                if (doc.RootElement.TryGetProperty("Token", out var el) && el.ValueKind == System.Text.Json.JsonValueKind.String)
                    token = el.GetString();
            }
            catch { }

            // 핸들러가 다시 읽을 수 있도록 위치 초기화
            context.Request.Body.Position = 0;
        }

        if (string.IsNullOrEmpty(token))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var tokenEntity = await userDb.UserTokens
            .FirstOrDefaultAsync(t => t.Token == token && t.ExpiresAt > DateTime.UtcNow);

        if (tokenEntity == null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        context.Items["Uid"] = tokenEntity.Uid;

        await _next(context);
    }
}
