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


var test = new TestRequest();
var haha = await client.PostAsJsonAsync("/TestRequest", test);
if (haha.IsSuccessStatusCode)
{
    var result = await haha.Content.ReadFromJsonAsync<TestResponse>();
    Console.WriteLine($"Test 성공! MessageId: {result?.MessageId}");
}
else
{
    Console.WriteLine($"Test 실패: {haha.StatusCode}");
}
