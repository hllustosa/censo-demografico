using Asp.Versioning;
using Census.Identity.Api.Contracts;
using Census.Identity.Infra.Entities;
using Census.Shared.Auth;
using Census.Shared.Web;
using Census.Shared.Web.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Census.Identity.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
[Authorize(Policy = CensusPolicies.CanManageUsers)]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    [EnableRateLimiting(RateLimitingExtensions.AuthenticatedPolicy)]
    [ProducesResponseType(typeof(PagedUsersResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedUsersResponse>> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var allUsers = _userManager.Users.ToList();
        var total = allUsers.Count;
        var items = new List<UserListItemResponse>();

        foreach (var user in allUsers.Skip((page - 1) * pageSize).Take(pageSize))
        {
            var roles = await _userManager.GetRolesAsync(user);
            items.Add(new UserListItemResponse(
                user.Id.ToString(),
                user.Email ?? string.Empty,
                user.FullName,
                roles,
                user.IsActive,
                user.CreatedAt));
        }

        return Ok(new PagedUsersResponse(items, page, total));
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserProfileResponse>> Create([FromBody] CreateUserRequest request)
    {
        var invalidRoles = request.Roles.Except(CensusRoles.All).ToList();
        if (invalidRoles.Count > 0)
        {
            throw new BusinessRuleException($"Roles inválidas: {string.Join(", ", invalidRoles)}");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true,
            FullName = request.FullName,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
            return ValidationProblem(new ValidationProblemDetails(errors)
            {
                Title = "Falha ao criar usuário",
                Detail = "Corrija os campos indicados."
            });
        }

        await _userManager.AddToRolesAsync(user, request.Roles);
        var roles = await _userManager.GetRolesAsync(user);

        return CreatedAtAction(nameof(Get), new { page = 1 },
            new UserProfileResponse(user.Id.ToString(), user.Email ?? string.Empty, user.FullName, roles));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileResponse>> Update(string id, [FromBody] UpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(id)
            ?? throw new NotFoundException("Usuário não encontrado.");

        var invalidRoles = request.Roles.Except(CensusRoles.All).ToList();
        if (invalidRoles.Count > 0)
        {
            throw new BusinessRuleException($"Roles inválidas: {string.Join(", ", invalidRoles)}");
        }

        user.FullName = request.FullName;
        user.IsActive = request.IsActive;
        await _userManager.UpdateAsync(user);

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRolesAsync(user, request.Roles);

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new UserProfileResponse(user.Id.ToString(), user.Email ?? string.Empty, user.FullName, roles));
    }

    [HttpPut("{id}/password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPassword(string id, [FromBody] ResetPasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(id)
            ?? throw new NotFoundException("Usuário não encontrado.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description });
            return ValidationProblem(new ValidationProblemDetails(errors)
            {
                Title = "Falha ao redefinir senha",
                Detail = "Corrija os campos indicados."
            });
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id)
            ?? throw new NotFoundException("Usuário não encontrado.");

        user.IsActive = false;
        await _userManager.UpdateAsync(user);
        return NoContent();
    }
}
