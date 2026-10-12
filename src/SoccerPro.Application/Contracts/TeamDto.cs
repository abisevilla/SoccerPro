namespace SoccerPro.Application;

public class TeamDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? City { get; set; }
    public DateTimeOffset? ModifiedAt { get; set; }
}
