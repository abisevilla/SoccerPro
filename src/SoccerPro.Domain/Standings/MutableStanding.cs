namespace SoccerPro.Domain;

internal class MutableStanding
{
    private readonly Team team;
    private int played;
    private int wins;
    private int draws;
    private int losses;
    private int goalsFor;
    private int goalsAgainst;
    private int points;

    public MutableStanding(Team team)
    {
        this.team = team;
    }

    public void Apply(int scored, int conceded)
    {
        played++;
        goalsFor += scored;
        goalsAgainst += conceded;

        if (scored > conceded)
        {
            wins++;
            points += 3;
        }
        else if (scored == conceded)
        {
            draws++;
            points++;
        }
        else
        {
            losses++;
        }
    }

    public Standing ToStanding() => new(
        team.Id,
        team.Name,
        played,
        wins,
        draws,
        losses,
        goalsFor,
        goalsAgainst,
        points);
}
