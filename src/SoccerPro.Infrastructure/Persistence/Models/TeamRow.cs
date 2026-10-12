namespace SoccerPro.Infrastructure.Persistence.Models;

public class TeamRow
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? ModifiedAt { get; set; }
}
