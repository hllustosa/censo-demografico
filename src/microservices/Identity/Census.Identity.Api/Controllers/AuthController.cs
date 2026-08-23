using Asp.Versioning;
using Census.Identity.Api.Contracts;
using Census.Identity.Api.Services;
using Census.Identity.Infra.Entities;
using Census.Shared.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Census.Identity.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.LoginPolicy)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.IsActive)
        {
            return Unauthorized(CreateUnauthorized("E-mail ou senha incorretos."));
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            return Unauthorized(CreateUnauthorized("E-mail ou senha incorretos."));
        }

        var roles = await _userManager.GetRolesAsync(user);
        var tokens = await _tokenService.CreateTokensAsync(user, roles);
        return Ok(MapAuthResponse(tokens));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.LoginPolicy)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshRequest request)
    {
        var tokens = await _tokenService.RefreshAsync(request.RefreshToken);
        if (tokens is null)
        {
            return Unauthorized(CreateUnauthorized("Refresh token inválido ou expirado."));
        }

        return Ok(MapAuthResponse(tokens));
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        await _tokenService.RevokeAsync(request.RefreshToken);
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    [EnableRateLimiting(RateLimitingExtensions.AuthenticatedPolicy)]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserProfileResponse>> Me()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null || !user.IsActive)
        {
            return Unauthorized();
        }

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new UserProfileResponse(user.Id.ToString(), user.Email ?? string.Empty, user.FullName, roles));
    }

    private static AuthResponse MapAuthResponse(AuthTokenResult tokens) =>
        new(tokens.AccessToken, tokens.RefreshToken, tokens.ExpiresAt,
            new UserProfileResponse(tokens.UserId, tokens.Email, tokens.FullName, tokens.Roles));

    private static ProblemDetails CreateUnauthorized(string detail) => new()
    {
        Type = "https://censo.local/errors/unauthorized",
        Title = "Credenciais inválidas",
        Status = StatusCodes.Status401Unauthorized,
        Detail = detail
    };
}
