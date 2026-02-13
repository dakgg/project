using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

var dbConfigPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", "config", "dev", "database.json");
var dbConfig = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(dbConfigPath));
var connectionString = $"Server={dbConfig.GetProperty("Server")};Port={dbConfig.GetProperty("Port")};Database={dbConfig.GetProperty("Database")};User={dbConfig.GetProperty("User")};Password={dbConfig.GetProperty("Password")};";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

var handlerTypes = HandlerHelper.FindHandlerTypes();
builder.Services.RegisterHandlers(handlerTypes);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapHandlers(handlerTypes);

app.Run();
