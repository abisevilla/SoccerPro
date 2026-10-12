using SoccerPro.Domain;
using Xunit;

namespace SoccerPro.Tests;

public sealed class StandingsCalculatorTests
{
    [Fact]
    public void Calculate_AssignsThreePointsAndGoalDifferenceForWin()
    {
        var home = new Team("Home", null);
        var away = new Team("Away", null);
        var match = new Match(home.Id, away.Id, DateTimeOffset.UtcNow);
        match.RecordResult(3, 1);
        var table = StandingsCalculator.Calculate(new[] { home, away }, new[] { match });
        Assert.Equal(home.Id, table[0].TeamId);
        Assert.Equal(3, table[0].Points);
        Assert.Equal(2, table[0].GoalDifference);
    }
    [Fact]
    public void Calculate_AssignsOnePointForDraw()
    {
        var home = new Team("Home", null);
        var away = new Team("Away", null);
        var match = new Match(home.Id, away.Id, DateTimeOffset.UtcNow);
        match.RecordResult(1, 1);
        var table = StandingsCalculator.Calculate(new[] { home, away }, new[] { match });
        Assert.All(table, standing => Assert.Equal(1, standing.Points));
    }
    [Fact]
    public void Result_FailureCarriesErrorCode()
    {
        var result = Result.Failure("Invalid", "validation");
        Assert.False(result.IsSuccess);
        Assert.Equal("validation", result.Error!.Code);
    }
}
