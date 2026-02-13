using System.Net.Http.Json;
using dakg.shared;

var client = new HttpClient { BaseAddress = new Uri("http://localhost:5031") };

var request = new LoginRequest
{
    PublicKey = "admin",
    PrivateKey = "password"
};

var response = await client.PostAsJsonAsync("/LoginRequest", request);

if (response.IsSuccessStatusCode)
{
    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
    Console.WriteLine($"Login 성공! MessageId: {result?.MessageId}");
}
else
{
    Console.WriteLine($"Login 실패: {response.StatusCode}");
}
