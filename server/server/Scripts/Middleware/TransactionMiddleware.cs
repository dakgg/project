using Serilog;

/// <summary>
/// Wraps each request in a transaction on UserDb and, if a handler enlists one,
/// on the relevant game DB shard via <see cref="GameShardTransactionContext"/>.
/// Opening transactions on all shards unconditionally causes unnecessary locks and
/// connection waste, so shards are opted in lazily by handlers.
/// </summary>
public class TransactionMiddleware
{
    private readonly RequestDelegate _next;

    public TransactionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        UserDbContext userDb,
        GameShardTransactionContext gameShardCtx)
    {
        await using var userTx = await userDb.Database.BeginTransactionAsync();

        try
        {
            await _next(context);

            await userDb.SaveChangesAsync();
            await userTx.CommitAsync();

            await gameShardCtx.CommitAsync();
        }
        catch (Exception ex)
        {
            await userTx.RollbackAsync();
            await gameShardCtx.RollbackAsync();
            Log.Error(ex, "Transaction rolled back");
            throw;
        }
    }
}
