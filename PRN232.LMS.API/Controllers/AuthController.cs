using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.RequestModels;
using PRN232.LMS.Services.ResponseModels;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[ApiVersionNeutral]
[Route("api/[controller]")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json", "application/xml")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Authenticate user credentials and retrieve access/refresh tokens.</summary>
    /// <param name="request">Login credentials</param>
    /// <param name="requestId">Request tracking identifier from header</param>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        [FromHeader(Name = "X-Request-Id")] string? requestId)
    {
        if (requestId != null)
        {
            HttpContext.Response.Headers["X-Response-Id"] = requestId;
        }

        var result = await _authService.LoginAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Refresh expired access token using a valid refresh token.</summary>
    /// <param name="request">Refresh token details</param>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Register a new user account.</summary>
    /// <param name="request">Registration details</param>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Endpoint that throws an unhandled exception to test the global exception middleware.</summary>
    [HttpGet("error")]
    [AllowAnonymous]
    public IActionResult ThrowError()
    {
        throw new System.Exception("Test global exception middleware.");
    }
}
