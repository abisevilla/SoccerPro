using Microsoft.EntityFrameworkCore;
using SoccerPro.Application;
using SoccerPro.Domain;

namespace SoccerPro.Infrastructure;

public sealed class EfWriteRepository<T> : IWriteRepository<T> where T : Entity
{
    private readonly SoccerProDbContext dbContext;

    public EfWriteRepository(SoccerProDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Set<T>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

    public Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
        dbContext.Set<T>().AddAsync(entity, cancellationToken).AsTask();

    public void Remove(T entity) => dbContext.Set<T>().Remove(entity);
}
