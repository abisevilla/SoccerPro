namespace SoccerPro.Domain;

public static class StandingsCalculator
{
    public static IReadOnlyList<Standing> Calculate(IEnumerable<Team> teams, IEnumerable<Match> matches)
    {
        var table = teams.ToDictionary(team => team.Id, team => new MutableStanding(team));

        foreach (var match in matches.Where(match => match.IsCompleted))
        {
            if (!table.TryGetValue(match.HomeTeamId, out var home) ||
                !table.TryGetValue(match.AwayTeamId, out var away))
            {
                continue;
            }

            home.Apply(match.HomeScore!.Value, match.AwayScore!.Value);
            away.Apply(match.AwayScore!.Value, match.HomeScore.Value);
        }

        return table.Values
            .Select(item => item.ToStanding())
            .OrderByDescending(item => item.Points)
            .ThenByDescending(item => item.GoalDifference)
            .ThenByDescending(item => item.GoalsFor)
            .ThenBy(item => item.TeamName)
            .ToList();
    }
}
