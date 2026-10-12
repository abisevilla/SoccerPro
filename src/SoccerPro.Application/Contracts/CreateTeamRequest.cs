namespace SoccerPro.Application;

public class CreateTeamRequest
{
    public required string Name { get; set; }
    public string? City { get; set; }
}
