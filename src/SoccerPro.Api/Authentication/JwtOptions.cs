namespace SoccerPro.Api.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Key { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int ExpirationMinutes { get; init; } = 60;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
