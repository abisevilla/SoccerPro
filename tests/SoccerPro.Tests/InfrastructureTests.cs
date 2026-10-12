using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SoccerPro.Application;
using SoccerPro.Domain;
using SoccerPro.Infrastructure;
using Xunit;

namespace SoccerPro.Tests;

public sealed class InfrastructureTests
{
    [Fact]
    public async Task UnitOfWork_CommitsRepositoryChanges()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<SoccerProDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext = new SoccerProDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();
        var repository = new EfWriteRepository<Team>(dbContext);
        var unitOfWork = new UnitOfWork(dbContext, NullLogger<UnitOfWork>.Instance);
        var team = new Team("Unit Test", "San Salvador");

        await repository.AddAsync(team);
        var result = await unitOfWork.CommitAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(team.Id, await dbContext.Teams.Select(item => item.Id).SingleAsync());
    }

    [Fact]
    public async Task IdempotencyStore_PersistsAndReadsRecordThroughUnitOfWork()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<SoccerProDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext = new SoccerProDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();
        var unitOfWork = new UnitOfWork(dbContext, NullLogger<UnitOfWork>.Instance);
        var store = new IdempotencyStore(dbContext, unitOfWork);
        var record = new IdempotencyRecord
        {
            Key = "test-key",
            RequestHash = "hash",
            StatusCode = 201,
            ResponseBody = "{}"
        };

        var saveResult = await store.SaveAsync(record);
        var stored = await store.GetAsync("test-key");

        Assert.True(saveResult.IsSuccess);
        Assert.NotNull(stored);
        Assert.Equal(201, stored.StatusCode);
    }
}
