using SoccerPro.Domain;

namespace SoccerPro.Application;

public interface IApplicationEventLogger
{
    void TeamCreated(Team team);
    void MatchResultRecorded(Match match);
}
