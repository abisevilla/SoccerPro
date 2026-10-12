using Microsoft.EntityFrameworkCore;
using SoccerPro.Domain;

namespace SoccerPro.Infrastructure;

public sealed class SoccerProDbContext : DbContext
{
    public SoccerProDbContext(DbContextOptions<SoccerProDbContext> options) : base(options)
    {
    }

    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Team>().HasIndex(team => team.Name).IsUnique();
        modelBuilder.Entity<IdempotencyRecord>().HasIndex(record => record.Key).IsUnique();
    }
}
