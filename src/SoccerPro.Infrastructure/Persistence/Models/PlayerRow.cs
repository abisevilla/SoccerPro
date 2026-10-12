namespace SoccerPro.Infrastructure.Persistence.Models;

public class PlayerRow
{
    public string Id { get; set; } = string.Empty;
    public string TeamId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string? ModifiedAt { get; set; }
}
