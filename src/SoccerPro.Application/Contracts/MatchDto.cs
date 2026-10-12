namespace SoccerPro.Application;

public class MatchDto
{
    public Guid Id { get; set; }
    public Guid HomeTeamId { get; set; }
    public Guid AwayTeamId { get; set; }
    public DateTimeOffset ScheduledAt { get; set; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public required string Status { get; set; }
    public DateTimeOffset? ModifiedAt { get; set; }
}
