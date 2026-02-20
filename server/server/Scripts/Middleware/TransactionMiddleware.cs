using Microsoft.EntityFrameworkCore;
using Serilog;

public class TransactionMiddleware
{
    private readonly RequestDelegate _next;

    public TransactionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, UserDbContext userDb, GameDbShardManager gameShards)
    {
        var dbs = new List<DbContext> { userDb };
        dbs.AddRange(gameShards.All);

        var transactions = await Task.WhenAll(dbs.Select(db => db.Database.BeginTransactionAsync()));

        try
        {
            await _next(context);
            foreach (var db in dbs)
                await db.SaveChangesAsync();
            foreach (var tx in transactions)
                await tx.CommitAsync();
        }
        catch (Exception ex)
        {
            foreach (var tx in transactions)
                await tx.RollbackAsync();
            Log.Error(ex, "Transaction rolled back");
            throw;
        }
    }
}
