using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Serilog;
using StackExchange.Redis;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

var configBase = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "config", "dev");
var dbConfig = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(Path.Combine(configBase, "database.json")));

// Day 7: 커넥션 풀링 설정 - 최소 5개 유지, 최대 20개, 유휴 180초 후 반환
static string BuildConnectionString(JsonElement config, string key)
{
    var db = config.GetProperty(key);
    return $"Server={db.GetProperty("Server")};" +
           $"Port={db.GetProperty("Port")};" +
           $"Database={db.GetProperty("Database")};" +
           $"User={db.GetProperty("User")};" +
           $"Password={db.GetProperty("Password")};" +
           "Pooling=true;MinimumPoolSize=5;MaximumPoolSize=20;" +
           "ConnectionIdleTimeout=180;AllowUserVariables=true;";
}

// User DB
var userConnStr = BuildConnectionString(dbConfig, "UserDb");
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseMySql(userConnStr, ServerVersion.AutoDetect(userConnStr)));

// Game DB 샤딩
var shardingCount = dbConfig.GetProperty("GameDbShardingCount").GetInt32();
var gameDbShardConfig = new GameDbShardConfig();
for (int i = 1; i <= shardingCount; i++)
{
    var connStr = BuildConnectionString(dbConfig, $"GameDb{i}");
    var serverVersion = ServerVersion.AutoDetect(connStr);
    gameDbShardConfig.Shards.Add((connStr, serverVersion));
}
builder.Services.AddSingleton(gameDbShardConfig);
builder.Services.AddScoped<GameDbShardManager>();
builder.Services.AddScoped<GameShardTransactionContext>();

// Redis (config.json에 Redis 설정이 없으면 스킵)
try
{
    var configPath = Path.Combine(configBase, "config.json");
    if (File.Exists(configPath))
    {
        var appConfig = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(configPath));
        if (appConfig.TryGetProperty("Redis", out var redisEl))
        {
            var redisConn = redisEl.GetProperty("ConnectionString").GetString()!;
            builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(redisConn));
            builder.Services.AddSingleton<RedisClient>();
            Log.Information("Redis connected: {Conn}", redisConn);
        }
    }
}
catch (Exception ex)
{
    Log.Warning(ex, "Redis 설정 없음 - 캐싱 비활성화");
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var handlerTypes = HandlerHelper.FindHandlerTypes();
builder.Services.RegisterHandlers(handlerTypes);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<TransactionMiddleware>();

app.MapGet("/", () => "Hello World!");
app.MapHandlers(handlerTypes);

app.Run();
