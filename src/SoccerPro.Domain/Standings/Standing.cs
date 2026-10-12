namespace SoccerPro.Domain;

public record Standing(
    Guid TeamId,
    string TeamName,
    int Played,
    int Wins,
    int Draws,
    int Losses,
    int GoalsFor,
    int GoalsAgainst,
    int Points)
{
    public int GoalDifference => GoalsFor - GoalsAgainst;
}
