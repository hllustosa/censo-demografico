namespace Census.Shared.Auth;

public static class CensusRoles
{
    public const string Registrar = "Registrar";
    public const string Analyst = "Analyst";
    public const string Admin = "Admin";

    public static readonly IReadOnlyList<string> All = [Registrar, Analyst, Admin];
}
