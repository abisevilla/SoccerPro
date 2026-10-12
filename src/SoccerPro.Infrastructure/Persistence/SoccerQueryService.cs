using Dapper;
using Microsoft.Data.Sqlite;
using SoccerPro.Application;
using SoccerPro.Domain;
using SoccerPro.Infrastructure.Persistence.Models;

namespace SoccerPro.Infrastructure;

public sealed class SoccerQueryService : ISoccerQueryService
{
    private readonly SqliteConnection connection;

    public SoccerQueryService(SqliteConnection connection)
    {
        this.connection = connection;
    }

    private static string Sort(string sortBy, string sortDirection, IReadOnlyDictionary<string, string> allowed, string fallback)
    {
        var column = allowed.TryGetValue(sortBy.ToLowerInvariant(), out var value) ? value : fallback;
        return $"{column} {(sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC")}";
    }

    public async Task<PagedResponse<TeamDto>> GetTeamsAsync(PageRequest page, string? name, CancellationToken cancellationToken = default)
    {
        var sql = "FROM Teams WHERE (@name IS NULL OR Name LIKE '%' || @name || '%')";
        var total = await connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) {sql}", new { name });
        var rows = await connection.QueryAsync<TeamRow>(
            $"SELECT Id, Name, City, ModifiedAt {sql} ORDER BY {Sort(page.SortBy, page.SortDirection, new Dictionary<string, string> { { "name", "Name" }, { "city", "City" } }, "Name")} LIMIT @limit OFFSET @offset",
            new { name, limit = page.SafePageSize, offset = (page.SafePageNumber - 1) * page.SafePageSize });

        return new PagedResponse<TeamDto>
        {
            Data = rows.Select(ToDto).ToList(),
            PageNumber = page.SafePageNumber,
            PageSize = page.SafePageSize,
            TotalRecords = total,
            TotalPages = (int)Math.Ceiling(total / (double)page.SafePageSize)
        };
    }

    public async Task<TeamDto?> GetTeamAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var row = await connection.QuerySingleOrDefaultAsync<TeamRow>(
            "SELECT Id, Name, City, ModifiedAt FROM Teams WHERE Id = @id",
            new { id = id.ToString() });
        return row is null ? null : ToDto(row);
    }

    public async Task<PagedResponse<PlayerDto>> GetPlayersAsync(PageRequest page, Guid? teamId, string? name, string? position, CancellationToken cancellationToken = default)
    {
        var sql = "FROM Players WHERE (@teamId IS NULL OR TeamId = @teamId) AND (@name IS NULL OR Name LIKE '%' || @name || '%') AND (@position IS NULL OR Position = @position)";
        var parameters = new { teamId = teamId?.ToString(), name, position, limit = page.SafePageSize, offset = (page.SafePageNumber - 1) * page.SafePageSize };
        var total = await connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) {sql}", parameters);
        var rows = await connection.QueryAsync<PlayerRow>(
            $"SELECT Id, TeamId, Name, Position, ModifiedAt {sql} ORDER BY {Sort(page.SortBy, page.SortDirection, new Dictionary<string, string> { { "name", "Name" }, { "position", "Position" } }, "Name")} LIMIT @limit OFFSET @offset",
            parameters);

        return new PagedResponse<PlayerDto>
        {
            Data = rows.Select(ToDto).ToList(),
            PageNumber = page.SafePageNumber,
            PageSize = page.SafePageSize,
            TotalRecords = total,
            TotalPages = (int)Math.Ceiling(total / (double)page.SafePageSize)
        };
    }

    public async Task<PlayerDto?> GetPlayerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var row = await connection.QuerySingleOrDefaultAsync<PlayerRow>(
            "SELECT Id, TeamId, Name, Position, ModifiedAt FROM Players WHERE Id = @id",
            new { id = id.ToString() });
        return row is null ? null : ToDto(row);
    }

    public async Task<PagedResponse<MatchDto>> GetMatchesAsync(PageRequest page, Guid? teamId, DateTimeOffset? from, DateTimeOffset? to, string? status, CancellationToken cancellationToken = default)
    {
        var sql = "FROM Matches WHERE (@teamId IS NULL OR HomeTeamId = @teamId OR AwayTeamId = @teamId) AND (@from IS NULL OR ScheduledAt >= @from) AND (@to IS NULL OR ScheduledAt <= @to) AND (@status IS NULL OR (@status = 'completed' AND HomeScore IS NOT NULL) OR (@status = 'scheduled' AND HomeScore IS NULL))";
        var parameters = new { teamId = teamId?.ToString(), from, to, status, limit = page.SafePageSize, offset = (page.SafePageNumber - 1) * page.SafePageSize };
        var total = await connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) {sql}", parameters);
        var rows = await connection.QueryAsync<MatchRow>(
            $"SELECT Id, HomeTeamId, AwayTeamId, ScheduledAt, HomeScore, AwayScore, ModifiedAt, CASE WHEN HomeScore IS NULL THEN 'scheduled' ELSE 'completed' END AS Status {sql} ORDER BY {Sort(page.SortBy, page.SortDirection, new Dictionary<string, string> { { "date", "ScheduledAt" }, { "scheduledat", "ScheduledAt" } }, "ScheduledAt")} LIMIT @limit OFFSET @offset",
            parameters);

        return new PagedResponse<MatchDto>
        {
            Data = rows.Select(ToDto).ToList(),
            PageNumber = page.SafePageNumber,
            PageSize = page.SafePageSize,
            TotalRecords = total,
            TotalPages = (int)Math.Ceiling(total / (double)page.SafePageSize)
        };
    }

    public async Task<MatchDto?> GetMatchAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var row = await connection.QuerySingleOrDefaultAsync<MatchRow>(
            "SELECT Id, HomeTeamId, AwayTeamId, ScheduledAt, HomeScore, AwayScore, ModifiedAt, CASE WHEN HomeScore IS NULL THEN 'scheduled' ELSE 'completed' END AS Status FROM Matches WHERE Id = @id",
            new { id = id.ToString() });
        return row is null ? null : ToDto(row);
    }

    public async Task<IReadOnlyList<Standing>> GetStandingsAsync(CancellationToken cancellationToken = default)
    {
        var teams = (await connection.QueryAsync<TeamRow>("SELECT Id, Name, City, ModifiedAt FROM Teams"))
            .Select(row => new Team(row.Name, row.City) { Id = Guid.Parse(row.Id) });
        var matches = (await connection.QueryAsync<MatchRow>("SELECT Id, HomeTeamId, AwayTeamId, ScheduledAt, HomeScore, AwayScore, ModifiedAt, CASE WHEN HomeScore IS NULL THEN 'scheduled' ELSE 'completed' END AS Status FROM Matches"))
            .Select(row => new Match(Guid.Parse(row.HomeTeamId), Guid.Parse(row.AwayTeamId), DateTimeOffset.Parse(row.ScheduledAt))
            {
                Id = Guid.Parse(row.Id),
                HomeScore = row.HomeScore,
                AwayScore = row.AwayScore
            });

        return StandingsCalculator.Calculate(teams, matches);
    }

    private static TeamDto ToDto(TeamRow row) => new()
    {
        Id = Guid.Parse(row.Id),
        Name = row.Name,
        City = row.City,
        ModifiedAt = ParseModifiedAt(row.ModifiedAt)
    };

    private static PlayerDto ToDto(PlayerRow row) => new()
    {
        Id = Guid.Parse(row.Id),
        TeamId = Guid.Parse(row.TeamId),
        Name = row.Name,
        Position = row.Position,
        ModifiedAt = ParseModifiedAt(row.ModifiedAt)
    };

    private static MatchDto ToDto(MatchRow row) => new()
    {
        Id = Guid.Parse(row.Id),
        HomeTeamId = Guid.Parse(row.HomeTeamId),
        AwayTeamId = Guid.Parse(row.AwayTeamId),
        ScheduledAt = DateTimeOffset.Parse(row.ScheduledAt),
        HomeScore = row.HomeScore,
        AwayScore = row.AwayScore,
        ModifiedAt = ParseModifiedAt(row.ModifiedAt),
        Status = row.Status
    };

    private static DateTimeOffset? ParseModifiedAt(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : DateTimeOffset.Parse(value);

}
