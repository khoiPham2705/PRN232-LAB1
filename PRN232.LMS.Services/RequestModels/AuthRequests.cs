using System.ComponentModel.DataAnnotations;
using PRN232.LMS.Services.Helpers;

namespace PRN232.LMS.Services.RequestModels;

public class LoginRequest
{
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = null!;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = null!;
}

public class RegisterRequest
{
    [Required]
    [FptuCode]
    public string Username { get; set; } = null!;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = null!;

    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = null!;
}

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = null!;
}
