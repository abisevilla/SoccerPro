using SoccerPro.Domain;
namespace SoccerPro.Application;

public class PlayerService
{
    private readonly IWriteRepository<Player> repository;
    private readonly IWriteRepository<Team> teamRepository;
    private readonly IUnitOfWork unitOfWork;

    public PlayerService(IWriteRepository<Player> repository, IWriteRepository<Team> teamRepository, IUnitOfWork unitOfWork)
    {
        this.repository = repository;
        this.teamRepository = teamRepository;
        this.unitOfWork = unitOfWork;
    }

    public async Task<Result<Player>> CreateAsync(CreatePlayerRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Position))
            return Result<Player>.Failure("Player name and position are required.", "validation");

        var team = await teamRepository.GetByIdAsync(request.TeamId, cancellationToken);
        if (team is null)
            return Result<Player>.Failure("Team was not found.", "not_found");

        var player = new Player(request.TeamId, request.Name.Trim(), request.Position.Trim());
        await repository.AddAsync(player, cancellationToken);
        var result = await unitOfWork.CommitAsync(cancellationToken);
        return result.IsSuccess
            ? Result<Player>.Success(player)
            : Result<Player>.Failure(result.Error!.Message, result.Error.Code);
    }

    public async Task<Result<Player>> UpdateAsync(Guid id, UpdatePlayerRequest request, CancellationToken cancellationToken = default)
    {
        var player = await repository.GetByIdAsync(id, cancellationToken);
        if (player is null)
            return Result<Player>.Failure("Player was not found.", "not_found");
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Position))
            return Result<Player>.Failure("Player name and position are required.", "validation");

        player.Update(request.Name.Trim(), request.Position.Trim());
        var result = await unitOfWork.CommitAsync(cancellationToken);
        return result.IsSuccess
            ? Result<Player>.Success(player)
            : Result<Player>.Failure(result.Error!.Message, result.Error.Code);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var player = await repository.GetByIdAsync(id, cancellationToken);
        if (player is not null)
            repository.Remove(player);

        return await unitOfWork.CommitAsync(cancellationToken);
    }
}
