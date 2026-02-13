using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

var handlerTypes = HandlerHelper.FindHandlerTypes();
builder.Services.RegisterHandlers(handlerTypes);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapHandlers(handlerTypes);

app.Run();
