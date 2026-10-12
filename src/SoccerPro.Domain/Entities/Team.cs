namespace SoccerPro.Domain;

public class Team : Entity
{
    public string Name { get; set; } = string.Empty;
    public string? City { get; set; }
    public DateTimeOffset? ModifiedAt { get; private set; }

    private Team() { }
    public Team(string name, string? city)
    {
        Name = name;
        City = city;
    }

    public void Update(string name, string? city)
    {
        Name = name;
        City = city;
        ModifiedAt = DateTimeOffset.UtcNow;
    }
}
