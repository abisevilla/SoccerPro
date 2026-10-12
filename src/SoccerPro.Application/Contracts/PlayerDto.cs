namespace SoccerPro.Application;

public class PlayerDto
{
    public Guid Id { get; set; }
    public Guid TeamId { get; set; }
    public required string Name { get; set; }
    public required string Position { get; set; }
    public DateTimeOffset? ModifiedAt { get; set; }
}
