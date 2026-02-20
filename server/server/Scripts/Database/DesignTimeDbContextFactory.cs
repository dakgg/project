using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class UserDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
{
    public UserDbContext CreateDbContext(string[] args)
    {
        var config = DesignTimeHelper.LoadDbConfig();
        var connStr = DesignTimeHelper.GetConnectionString(config, "UserDb");
        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseMySql(connStr, ServerVersion.AutoDetect(connStr))
            .Options;
        return new UserDbContext(options);
    }
}

public class GameDbContextFactory : IDesignTimeDbContextFactory<GameDbContext>
{
    public GameDbContext CreateDbContext(string[] args)
    {
        // 기본값 GameDb1, args로 오버라이드 가능
        var shardKey = args.Length > 0 ? args[0] : "GameDb1";
        var config = DesignTimeHelper.LoadDbConfig();
        var connStr = DesignTimeHelper.GetConnectionString(config, shardKey);
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseMySql(connStr, ServerVersion.AutoDetect(connStr))
            .Options;
        return new GameDbContext(options);
    }
}

internal static class DesignTimeHelper
{
    // design time 실행 경로: project/server/server
    private static readonly string ConfigPath = Path.Combine(
        Directory.GetCurrentDirectory(), "..", "..", "config", "dev", "database.json");

    public static JsonElement LoadDbConfig()
        => JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(ConfigPath));

    public static string GetConnectionString(JsonElement config, string key)
    {
        var db = config.GetProperty(key);
        return $"Server={db.GetProperty("Server")};Port={db.GetProperty("Port")};Database={db.GetProperty("Database")};User={db.GetProperty("User")};Password={db.GetProperty("Password")};";
    }
}
