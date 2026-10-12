using Microsoft.EntityFrameworkCore;
using SoccerPro.Application;
using SoccerPro.Domain;

namespace SoccerPro.Infrastructure;

public static class SeedData
{
    public static async Task InitializeAsync(SoccerProDbContext dbContext, IUnitOfWork unitOfWork)
    {
        if (await dbContext.Teams.AnyAsync())
            return;

        var teams = new[]
        {
            new Team("Lions", "San Salvador"),
            new Team("Tigers", "Santa Ana"),
            new Team("Eagles", "San Miguel"),
            new Team("Sharks", "La Libertad")
        };
        dbContext.Teams.AddRange(teams);

        foreach (var team in teams)
        {
            for (var number = 1; number <= 5; number++)
            {
                dbContext.Players.Add(new Player(team.Id, $"{team.Name} Player {number}", number == 1 ? "Goalkeeper" : "Forward"));
            }
        }

        var matches = new[]
        {
            new Match(teams[0].Id, teams[1].Id, DateTimeOffset.UtcNow.AddDays(-3)),
            new Match(teams[2].Id, teams[3].Id, DateTimeOffset.UtcNow.AddDays(-2)),
            new Match(teams[0].Id, teams[2].Id, DateTimeOffset.UtcNow.AddDays(-1)),
            new Match(teams[1].Id, teams[3].Id, DateTimeOffset.UtcNow.AddDays(1)),
            new Match(teams[0].Id, teams[3].Id, DateTimeOffset.UtcNow.AddDays(2)),
            new Match(teams[1].Id, teams[2].Id, DateTimeOffset.UtcNow.AddDays(3))
        };
        matches[0].RecordResult(2, 1);
        matches[1].RecordResult(0, 0);
        matches[2].RecordResult(1, 3);
        dbContext.Matches.AddRange(matches);
        await unitOfWork.CommitAsync();
    }
}
