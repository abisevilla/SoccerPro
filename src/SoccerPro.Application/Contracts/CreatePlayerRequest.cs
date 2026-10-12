namespace SoccerPro.Application;

public class CreatePlayerRequest
{
    public Guid TeamId { get; set; }
    public required string Name { get; set; }
    public required string Position { get; set; }
}
