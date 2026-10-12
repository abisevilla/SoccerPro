namespace SoccerPro.Application;

public class CreateMatchRequest
{
    public Guid HomeTeamId { get; set; }
    public Guid AwayTeamId { get; set; }
    public DateTimeOffset ScheduledAt { get; set; }
}
