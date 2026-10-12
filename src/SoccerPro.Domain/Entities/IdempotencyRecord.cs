namespace SoccerPro.Domain;

public class IdempotencyRecord : Entity
{
    public string Key { get; init; } = string.Empty;
    public string RequestHash { get; init; } = string.Empty;
    public int StatusCode { get; init; }
    public string ResponseBody { get; init; } = string.Empty;
    public string? Location { get; init; }
}
