using Microsoft.EntityFrameworkCore;
using SoccerPro.Application;
using SoccerPro.Domain;

namespace SoccerPro.Infrastructure;

public sealed class IdempotencyStore : IIdempotencyStore
{
    private readonly SoccerProDbContext dbContext;
    private readonly IUnitOfWork unitOfWork;

    public IdempotencyStore(SoccerProDbContext dbContext, IUnitOfWork unitOfWork)
    {
        this.dbContext = dbContext;
        this.unitOfWork = unitOfWork;
    }

    public Task<IdempotencyRecord?> GetAsync(string key, CancellationToken cancellationToken = default) =>
        dbContext.IdempotencyRecords.AsNoTracking().FirstOrDefaultAsync(record => record.Key == key, cancellationToken);

    public Task<Result> SaveAsync(IdempotencyRecord record, CancellationToken cancellationToken = default)
    {
        dbContext.IdempotencyRecords.Add(record);
        return unitOfWork.CommitAsync(cancellationToken);
    }
}
