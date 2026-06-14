using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.RequestModels;
using PRN232.LMS.Services.ResponseModels;

namespace PRN232.LMS.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly LmsDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(LmsDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower());

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return ApiResponse<AuthResponse>.Fail("Invalid username or password.");
        }

        var authResponse = await GenerateAuthResponseAsync(user);
        return ApiResponse<AuthResponse>.Ok(authResponse, "Login successful.");
    }

    public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var storedToken = await _context.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken);

        if (storedToken == null || !storedToken.IsActive)
        {
            return ApiResponse<AuthResponse>.Fail("Invalid or expired refresh token.");
        }

        // Revoke the old token
        storedToken.IsRevoked = true;
        _context.RefreshTokens.Update(storedToken);

        // Generate new token pair
        var authResponse = await GenerateAuthResponseAsync(storedToken.User);
        return ApiResponse<AuthResponse>.Ok(authResponse, "Token refreshed successfully.");
    }

    public async Task<ApiResponse<UserResponse>> RegisterAsync(RegisterRequest request)
    {
        var existing = await _context.Users
            .AnyAsync(u => u.Username.ToLower() == request.Username.ToLower());
        if (existing)
        {
            return ApiResponse<UserResponse>.Fail("Username already exists.");
        }

        var user = new User
        {
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var response = new UserResponse
        {
            UserId = user.UserId,
            Username = user.Username,
            Role = user.Role
        };

        return ApiResponse<UserResponse>.Ok(response, "User registered successfully.");
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(User user)
    {
        var secret = _configuration["JWT_SECRET"] ?? _configuration["Jwt:Secret"] ?? "ThisIsAVeryLongAndSecureSecretKeyForPRN232Lab2LmsAPI!";
        var issuer = _configuration["JWT_ISSUER"] ?? _configuration["Jwt:Issuer"] ?? "PRN232LmsAPI";
        var audience = _configuration["JWT_AUDIENCE"] ?? _configuration["Jwt:Audience"] ?? "PRN232LmsClient";
        var expiryMinutesStr = _configuration["Jwt:ExpiryMinutes"] ?? "60";
        int.TryParse(expiryMinutesStr, out var expiryMinutes);
        if (expiryMinutes <= 0) expiryMinutes = 60;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var expires = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var newRefreshToken = GenerateSecureToken();

        // Save new refresh token to DB
        var refreshTokenEntity = new RefreshToken
        {
            Token = newRefreshToken,
            UserId = user.UserId,
            ExpiryDate = DateTime.UtcNow.AddDays(7), // Refresh token expires in 7 days
            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = expiryMinutes * 60,
            User = new UserResponse
            {
                UserId = user.UserId,
                Username = user.Username,
                Role = user.Role
            }
        };
    }

    private string GenerateSecureToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
