using System.Reflection;
using dakg.shared;
using Serilog;

public static class HandlerHelper
{
    public static IEnumerable<Type> FindHandlerTypes()
    {
        return Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Handler"));
    }

    public static void RegisterHandlers(this IServiceCollection services, IEnumerable<Type> handlerTypes)
    {
        foreach (var handlerType in handlerTypes)
        {
            services.AddScoped(handlerType);
        }
    }

    public static void MapHandlers(this WebApplication app, IEnumerable<Type> handlerTypes)
    {
        foreach (var handlerType in handlerTypes)
        {
            var methods = handlerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (var method in methods)
            {
                var parameters = method.GetParameters();
                if (parameters.Length == 0) continue;

                var requestType = parameters[0].ParameterType;
                if (!requestType.IsSubclassOf(typeof(RequestBase))) continue;

                var route = $"/{requestType.Name}";

                var responseType = method.ReturnType;
                if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Task<>))
                    responseType = responseType.GetGenericArguments()[0];

                app.MapPost(route, async (HttpContext context) =>
                {
                    var request = await context.Request.ReadFromJsonAsync(requestType);
                    if (request == null) return Results.BadRequest();

                    var handler = context.RequestServices.GetRequiredService(handlerType);
                    var result = method.Invoke(handler, [request]);

                    if (result is Task task)
                    {
                        await task;
                        var taskResult = ((dynamic)task).Result;
                        return Results.Ok(taskResult);
                    }

                    return Results.Ok(result);
                })
                .Accepts(requestType, "application/json")
                .Produces(200, responseType)
                .WithName(method.Name)
                .WithTags(handlerType.Name.Replace("Handler", ""));

                Log.Information("Mapped POST {Route} -> {Handler}.{Method}", route, handlerType.Name, method.Name);
            }
        }
    }
}
