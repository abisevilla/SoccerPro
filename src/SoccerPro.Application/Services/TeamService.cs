using SoccerPro.Domain;
namespace SoccerPro.Application;

public class TeamService
{
    private readonly IWriteRepository<Team> repository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IApplicationEventLogger eventLogger;

    public TeamService(IWriteRepository<Team> repository, IUnitOfWork unitOfWork, IApplicationEventLogger eventLogger)
    {
        this.repository = repository;
        this.unitOfWork = unitOfWork;
        this.eventLogger = eventLogger;
    }

    public async Task<Result<Team>> CreateAsync(CreateTeamRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result<Team>.Failure("Team name is required.", "validation");

        var team = new Team(request.Name.Trim(), request.City?.Trim());
        await repository.AddAsync(team, cancellationToken);
        var result = await unitOfWork.CommitAsync(cancellationToken);
        if (!result.IsSuccess)
            return Result<Team>.Failure(result.Error!.Message, result.Error.Code);

        eventLogger.TeamCreated(team);
        return Result<Team>.Success(team);
    }

    public async Task<Result<Team>> UpdateAsync(Guid id, UpdateTeamRequest request, CancellationToken cancellationToken = default)
    {
        var team = await repository.GetByIdAsync(id, cancellationToken);
        if (team is null)
            return Result<Team>.Failure("Team was not found.", "not_found");
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result<Team>.Failure("Team name is required.", "validation");

        team.Update(request.Name.Trim(), request.City?.Trim());
        var result = await unitOfWork.CommitAsync(cancellationToken);
        return result.IsSuccess
            ? Result<Team>.Success(team)
            : Result<Team>.Failure(result.Error!.Message, result.Error.Code);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var team = await repository.GetByIdAsync(id, cancellationToken);
        if (team is not null)
            repository.Remove(team);

        return await unitOfWork.CommitAsync(cancellationToken);
    }
}
