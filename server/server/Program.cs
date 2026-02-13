
using dakg.shared;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/LoginRequest", (LoginRequest request) =>
{
    Console.WriteLine("LoginRequest received");
    // 간단한 인증 로직 (예: 하드코딩된 사용자명과 비밀번호 확인)
    if (request.PublicKey == "admin" && request.PrivateKey == "password")
    {
        return Results.Ok(new LoginResponse { });
    }
    else
    {
        return Results.Unauthorized();
    }
});

app.Run();

