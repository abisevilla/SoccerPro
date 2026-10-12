using Microsoft.Extensions.Logging;
using SoccerPro.Application;
using SoccerPro.Domain;

namespace SoccerPro.Infrastructure;

public sealed class ApplicationEventLogger : IApplicationEventLogger
{
    private readonly ILogger<ApplicationEventLogger> logger;

    public ApplicationEventLogger(ILogger<ApplicationEventLogger> logger)
    {
        this.logger = logger;
    }

    public void TeamCreated(Team team) =>
        logger.LogInformation("Domain event TeamCreated for team {TeamId} with name {TeamName}", team.Id, team.Name);

    public void MatchResultRecorded(Match match) =>
        logger.LogInformation("Domain event MatchResultRecorded for match {MatchId}: {HomeScore}-{AwayScore}", match.Id, match.HomeScore, match.AwayScore);
}
