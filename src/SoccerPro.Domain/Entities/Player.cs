namespace SoccerPro.Domain;

public class Player : Entity
{
    public Guid TeamId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public DateTimeOffset? ModifiedAt { get; private set; }

    private Player() { }
    public Player(Guid teamId, string name, string position)
    {
        TeamId = teamId;
        Name = name;
        Position = position;
    }

    public void Update(string name, string position)
    {
        Name = name;
        Position = position;
        ModifiedAt = DateTimeOffset.UtcNow;
    }
}
