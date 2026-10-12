using SoccerPro.Domain;

namespace SoccerPro.Application;

public interface IIdempotencyStore
{
    Task<IdempotencyRecord?> GetAsync(string key, CancellationToken cancellationToken = default);
    Task<Result> SaveAsync(IdempotencyRecord record, CancellationToken cancellationToken = default);
}
