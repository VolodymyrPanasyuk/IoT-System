using IoT_System.Application.Common;
using IoT_System.Application.DTOs.Request;
using IoT_System.Application.DTOs.Response.Auth;
using IoT_System.Application.Interfaces.Services.Auth;
using IoT_System.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IoT_System.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route($"{Constants.ApiRoutes.Identity}/[controller]")]
[ApiExplorerSettings(GroupName = Constants.SwaggerGroups.Identity)]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;

    public AuthController(IAuthService authService, IConfiguration configuration)
    {
        _authService = authService;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (result.IsSuccess && !string.IsNullOrWhiteSpace(result.Data.refreshToken))
        {
            SetRefreshTokenCookie(result.Data.refreshToken);
        }

        return result.ToResult(data => data.response);
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        if (result.IsSuccess && !string.IsNullOrWhiteSpace(result.Data.refreshToken))
        {
            SetRefreshTokenCookie(result.Data.refreshToken);
        }

        return result.ToResult(data => data.response);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new { message = "Refresh token not found" });
        }

        var result = await _authService.RefreshTokenAsync(refreshToken);
        if (result.IsSuccess && !string.IsNullOrWhiteSpace(result.Data.refreshToken))
        {
            SetRefreshTokenCookie(result.Data.refreshToken);
        }

        return result.ToResult(data => data.response);
    }

    [Authorize]
    [HttpPost("logout")]
    public ActionResult Logout()
    {
        Response.Cookies.Delete("refreshToken");
        return Ok(new { message = "Logged out successfully" });
    }

    #region Private Methods

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:RefreshTokenLifetimeDays"]!))
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }

    #endregion
}