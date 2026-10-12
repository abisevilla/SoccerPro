using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SoccerPro.Application;
using SoccerPro.Domain;

namespace SoccerPro.Infrastructure;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly SoccerProDbContext dbContext;
    private readonly ILogger<UnitOfWork> logger;

    public UnitOfWork(SoccerProDbContext dbContext, ILogger<UnitOfWork> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public async Task<Result> CommitAsync(CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateException exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(exception, "Database commit failed and the transaction was rolled back");
            return Result.Failure("The operation conflicts with existing data.", "conflict");
        }
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
