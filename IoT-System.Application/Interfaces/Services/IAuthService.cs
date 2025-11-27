using IoT_System.Application.DTOs.Request;
using IoT_System.Application.DTOs.Response.Auth;
using IoT_System.Application.Models;

namespace IoT_System.Application.Interfaces.Services;

public interface IAuthService
{
    Task<OperationResult<(AuthResponse response, string refreshToken)>> LoginAsync(LoginRequest request);
    Task<OperationResult<(AuthResponse response, string refreshToken)>> RegisterAsync(RegisterRequest request);
    Task<OperationResult<(AuthResponse response, string refreshToken)>> RefreshTokenAsync(string refreshToken);
}