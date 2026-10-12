using SoccerPro.Domain;
namespace SoccerPro.Application;

public class MatchService
{
    private readonly IWriteRepository<Match> repository;
    private readonly IWriteRepository<Team> teamRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IApplicationEventLogger eventLogger;

    public MatchService(IWriteRepository<Match> repository, IWriteRepository<Team> teamRepository, IUnitOfWork unitOfWork, IApplicationEventLogger eventLogger)
    {
        this.repository = repository;
        this.teamRepository = teamRepository;
        this.unitOfWork = unitOfWork;
        this.eventLogger = eventLogger;
    }

    public async Task<Result<Match>> CreateAsync(CreateMatchRequest request, CancellationToken cancellationToken = default)
    {
        if (request.HomeTeamId == request.AwayTeamId)
            return Result<Match>.Failure("Teams must be different.", "validation");

        var home = await teamRepository.GetByIdAsync(request.HomeTeamId, cancellationToken);
        var away = await teamRepository.GetByIdAsync(request.AwayTeamId, cancellationToken);
        if (home is null || away is null)
            return Result<Match>.Failure("A team was not found.", "not_found");

        var match = new Match(request.HomeTeamId, request.AwayTeamId, request.ScheduledAt);
        await repository.AddAsync(match, cancellationToken);
        var result = await unitOfWork.CommitAsync(cancellationToken);
        if (!result.IsSuccess)
            return Result<Match>.Failure(result.Error!.Message, result.Error.Code);

        return Result<Match>.Success(match);
    }

    public async Task<Result<Match>> UpdateAsync(Guid id, UpdateMatchRequest request, CancellationToken cancellationToken = default)
    {
        if (request.HomeTeamId == request.AwayTeamId)
            return Result<Match>.Failure("Teams must be different.", "validation");

        var match = await repository.GetByIdAsync(id, cancellationToken);
        if (match is null)
            return Result<Match>.Failure("Match was not found.", "not_found");

        var home = await teamRepository.GetByIdAsync(request.HomeTeamId, cancellationToken);
        var away = await teamRepository.GetByIdAsync(request.AwayTeamId, cancellationToken);
        if (home is null || away is null)
            return Result<Match>.Failure("A team was not found.", "not_found");

        match.Update(request.HomeTeamId, request.AwayTeamId, request.ScheduledAt);
        var result = await unitOfWork.CommitAsync(cancellationToken);
        return result.IsSuccess
            ? Result<Match>.Success(match)
            : Result<Match>.Failure(result.Error!.Message, result.Error.Code);
    }

    public async Task<Result<Match>> RecordResultAsync(Guid id, RecordResultRequest request, CancellationToken cancellationToken = default)
    {
        if (request.HomeScore < 0 || request.AwayScore < 0)
            return Result<Match>.Failure("Scores cannot be negative.", "validation");

        var match = await repository.GetByIdAsync(id, cancellationToken);
        if (match is null)
            return Result<Match>.Failure("Match was not found.", "not_found");

        match.RecordResult(request.HomeScore, request.AwayScore);
        var result = await unitOfWork.CommitAsync(cancellationToken);
        if (!result.IsSuccess)
            return Result<Match>.Failure(result.Error!.Message, result.Error.Code);

        eventLogger.MatchResultRecorded(match);
        return Result<Match>.Success(match);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var match = await repository.GetByIdAsync(id, cancellationToken);
        if (match is not null)
            repository.Remove(match);

        return await unitOfWork.CommitAsync(cancellationToken);
    }
}
