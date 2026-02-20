using Microsoft.EntityFrameworkCore;
using Serilog;

public class TransactionMiddleware
{
    private readonly RequestDelegate _next;

    public TransactionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            await _next(context);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Log.Error(ex, "Transaction rolled back");
            throw;
        }
    }
}
