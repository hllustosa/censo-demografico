namespace Census.Identity.Api.Contracts;

public record LoginRequest(string Email, string Password);

public record RefreshRequest(string RefreshToken);

public record LogoutRequest(string RefreshToken);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserProfileResponse User);

public record UserProfileResponse(
    string Id,
    string Email,
    string FullName,
    IEnumerable<string> Roles);

public record CreateUserRequest(
    string Email,
    string Password,
    string FullName,
    IEnumerable<string> Roles);

public record UpdateUserRequest(
    string FullName,
    IEnumerable<string> Roles,
    bool IsActive);

public record ResetPasswordRequest(string Password);

public record UserListItemResponse(
    string Id,
    string Email,
    string FullName,
    IEnumerable<string> Roles,
    bool IsActive,
    DateTime CreatedAt);

public record PagedUsersResponse(
    IEnumerable<UserListItemResponse> Items,
    int Page,
    int TotalItems);
