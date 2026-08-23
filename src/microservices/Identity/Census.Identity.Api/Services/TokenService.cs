using System.Security.Claims;
using System.Security.Cryptography;
using Census.Identity.Infra.Entities;
using Census.Shared.Auth;
using Census.Shared.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;

namespace Census.Identity.Api.Services;

public interface ITokenService
{
    Task<AuthTokenResult> CreateTokensAsync(ApplicationUser user, IEnumerable<string> roles);
    Task<AuthTokenResult?> RefreshAsync(string refreshToken);
    Task RevokeAsync(string refreshToken);
}

public record AuthTokenResult(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string UserId,
    string Email,
    string FullName,
    IEnumerable<string> Roles);

public class TokenService : ITokenService
{
    private readonly JwtOptions _jwtOptions;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMongoCollection<RefreshToken> _refreshTokens;

    public TokenService(
        IOptions<JwtOptions> jwtOptions,
        UserManager<ApplicationUser> userManager,
        IMongoCollection<RefreshToken> refreshTokens)
    {
        _jwtOptions = jwtOptions.Value;
        _userManager = userManager;
        _refreshTokens = refreshTokens;
    }

    public async Task<AuthTokenResult> CreateTokensAsync(ApplicationUser user, IEnumerable<string> roles)
    {
        var roleList = roles.ToList();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.FullName),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString())
        };

        foreach (var role in roleList)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var accessToken = AuthExtensions.GenerateAccessToken(_jwtOptions, claims);
        var refreshTokenValue = GenerateSecureToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes);

        await _refreshTokens.InsertOneAsync(new RefreshToken
        {
            UserId = user.Id.ToString(),
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays)
        });

        return new AuthTokenResult(
            accessToken,
            refreshTokenValue,
            expiresAt,
            user.Id.ToString(),
            user.Email ?? string.Empty,
            user.FullName,
            roleList);
    }

    public async Task<AuthTokenResult?> RefreshAsync(string refreshToken)
    {
        var storedToken = await _refreshTokens.Find(t => t.Token == refreshToken && !t.IsRevoked).FirstOrDefaultAsync();
        if (storedToken is null || storedToken.ExpiresAt <= DateTime.UtcNow)
        {
            return null;
        }

        var user = await _userManager.FindByIdAsync(storedToken.UserId);
        if (user is null || !user.IsActive)
        {
            return null;
        }

        storedToken.IsRevoked = true;
        await _refreshTokens.ReplaceOneAsync(t => t.Id == storedToken.Id, storedToken);

        var roles = await _userManager.GetRolesAsync(user);
        return await CreateTokensAsync(user, roles);
    }

    public async Task RevokeAsync(string refreshToken)
    {
        var storedToken = await _refreshTokens.Find(t => t.Token == refreshToken && !t.IsRevoked).FirstOrDefaultAsync();
        if (storedToken is null)
        {
            return;
        }

        storedToken.IsRevoked = true;
        await _refreshTokens.ReplaceOneAsync(t => t.Id == storedToken.Id, storedToken);
    }

    private static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}
