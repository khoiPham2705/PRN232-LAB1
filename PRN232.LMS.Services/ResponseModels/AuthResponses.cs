namespace PRN232.LMS.Services.ResponseModels;

public class UserResponse
{
    public int UserId { get; set; }
    public string Username { get; set; } = null!;
    public string Role { get; set; } = null!;
}

public class AuthResponse
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public int ExpiresIn { get; set; }
    public UserResponse User { get; set; } = null!;
}
