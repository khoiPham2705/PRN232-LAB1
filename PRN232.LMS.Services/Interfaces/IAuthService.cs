using PRN232.LMS.Services.RequestModels;
using PRN232.LMS.Services.ResponseModels;

namespace PRN232.LMS.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request);
    Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request);
    Task<ApiResponse<UserResponse>> RegisterAsync(RegisterRequest request);
}
