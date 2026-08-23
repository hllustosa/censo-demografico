namespace Census.Shared.Auth;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SigningKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "census-identity";
    public string Audience { get; set; } = "census-api";
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
}
