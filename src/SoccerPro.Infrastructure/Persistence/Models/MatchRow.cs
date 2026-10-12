namespace SoccerPro.Infrastructure.Persistence.Models;

public class MatchRow
{
    public string Id { get; set; } = string.Empty;
    public string HomeTeamId { get; set; } = string.Empty;
    public string AwayTeamId { get; set; } = string.Empty;
    public string ScheduledAt { get; set; } = string.Empty;
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ModifiedAt { get; set; }
}
