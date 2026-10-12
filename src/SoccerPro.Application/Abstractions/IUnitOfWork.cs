using SoccerPro.Domain;
namespace SoccerPro.Application;
public interface IUnitOfWork
{
    Task<Result> CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
