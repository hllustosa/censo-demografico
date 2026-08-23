using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Census.Shared.Web;
using Microsoft.IdentityModel.Tokens;

namespace Census.Shared.Auth;

public static class TestJwtHelper
{
    public const string DefaultSigningKey = "CensusDevSigningKeyMustBeAtLeast32CharsLong!";

    public static string CreateToken(params string[] roles)
    {
        var options = new JwtOptions
        {
            SigningKey = DefaultSigningKey,
            Issuer = "census-identity",
            Audience = "census-api"
        };

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(ClaimTypes.Email, "test@censo.local"),
            new(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return AuthExtensions.GenerateAccessToken(options, claims);
    }
}
