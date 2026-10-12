using SoccerPro.Domain;
namespace SoccerPro.Application;
public interface ISoccerQueryService
{
    Task<PagedResponse<TeamDto>> GetTeamsAsync(PageRequest page, string? name, CancellationToken cancellationToken = default);
    Task<TeamDto?> GetTeamAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResponse<PlayerDto>> GetPlayersAsync(PageRequest page, Guid? teamId, string? name, string? position, CancellationToken cancellationToken = default);
    Task<PlayerDto?> GetPlayerAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResponse<MatchDto>> GetMatchesAsync(PageRequest page, Guid? teamId, DateTimeOffset? from, DateTimeOffset? to, string? status, CancellationToken cancellationToken = default);
    Task<MatchDto?> GetMatchAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Standing>> GetStandingsAsync(CancellationToken cancellationToken = default);
}
