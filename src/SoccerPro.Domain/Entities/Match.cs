namespace SoccerPro.Domain;

public class Match : Entity
{
    public Guid HomeTeamId { get; set; }
    public Guid AwayTeamId { get; set; }
    public DateTimeOffset ScheduledAt { get; set; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public DateTimeOffset? ModifiedAt { get; private set; }
    public bool IsCompleted => HomeScore.HasValue && AwayScore.HasValue;

    private Match() { }
    public Match(Guid homeTeamId, Guid awayTeamId, DateTimeOffset scheduledAt)
    {
        HomeTeamId = homeTeamId;
        AwayTeamId = awayTeamId;
        ScheduledAt = scheduledAt;
    }

    public void Update(Guid homeTeamId, Guid awayTeamId, DateTimeOffset scheduledAt)
    {
        HomeTeamId = homeTeamId;
        AwayTeamId = awayTeamId;
        ScheduledAt = scheduledAt;
        ModifiedAt = DateTimeOffset.UtcNow;
    }

    public void RecordResult(int homeScore, int awayScore)
    {
        HomeScore = homeScore;
        AwayScore = awayScore;
        ModifiedAt = DateTimeOffset.UtcNow;
    }
}
