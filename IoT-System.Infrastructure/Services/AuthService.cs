using IoT_System.Application.DTOs.Request;
using IoT_System.Application.DTOs.Response.Auth;
using IoT_System.Application.Interfaces.Repositories;
using IoT_System.Application.Interfaces.Services;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace IoT_System.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly JwtService _jwtService;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<User> userManager,
        IRefreshTokenRepository refreshTokenRepository,
        JwtService jwtService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtService = jwtService;
        _configuration = configuration;
    }

    public async Task<OperationResult<(AuthResponse response, string refreshToken)>> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);
        if (user == null)
        {
            return OperationResult<(AuthResponse, string)>.Unauthorized("Invalid username or password");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            return OperationResult<(AuthResponse, string)>.Unauthorized("Invalid username or password");
        }

        var roles = await GetUserRolesAsync(user);
        var accessToken = _jwtService.GenerateAccessToken(user, roles);
        var refreshToken = await GenerateAndSaveRefreshTokenAsync(user.Id);

        var response = new AuthResponse
        {
            AccessToken = accessToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:AccessTokenLifetimeMinutes"]!))
        };

        return OperationResult<(AuthResponse, string)>.Success((response, refreshToken));
    }

    public async Task<OperationResult<(AuthResponse response, string refreshToken)>> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByNameAsync(request.UserName);
        if (existingUser != null)
        {
            return OperationResult<(AuthResponse, string)>.Conflict("User with this username already exists");
        }

        var user = new User
        {
            UserName = request.UserName,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return OperationResult<(AuthResponse, string)>.BadRequest(errors);
        }

        var roles = await GetUserRolesAsync(user);
        var accessToken = _jwtService.GenerateAccessToken(user, roles);
        var refreshToken = await GenerateAndSaveRefreshTokenAsync(user.Id);

        var response = new AuthResponse
        {
            AccessToken = accessToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:AccessTokenLifetimeMinutes"]!))
        };

        return OperationResult<(AuthResponse, string)>.Created((response, refreshToken));
    }

    public async Task<OperationResult<(AuthResponse response, string refreshToken)>> RefreshTokenAsync(string refreshToken)
    {
        var tokenResult = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
        if (!tokenResult.IsSuccess || tokenResult.Data == null)
        {
            return OperationResult<(AuthResponse, string)>.Unauthorized("Invalid refresh token");
        }

        var token = tokenResult.Data;

        if (token.ExpiryDate < DateTime.UtcNow)
        {
            await _refreshTokenRepository.DeleteAsync(token);
            return OperationResult<(AuthResponse, string)>.Unauthorized("Refresh token has expired");
        }

        var user = await _userManager.FindByIdAsync(token.UserId.ToString());
        if (user == null)
        {
            return OperationResult<(AuthResponse, string)>.NotFound("User not found");
        }

        await _refreshTokenRepository.DeleteAsync(token);

        var roles = await GetUserRolesAsync(user);
        var newAccessToken = _jwtService.GenerateAccessToken(user, roles);
        var newRefreshToken = await GenerateAndSaveRefreshTokenAsync(user.Id);

        var response = new AuthResponse
        {
            AccessToken = newAccessToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:AccessTokenLifetimeMinutes"]!))
        };

        return OperationResult<(AuthResponse, string)>.Success((response, newRefreshToken));
    }

    #region Private Methods

    private async Task<List<string>> GetUserRolesAsync(User user)
    {
        var userWithGroups = await _userManager.Users
            .Include(u => u.UserGroups)
            .ThenInclude(ug => ug.Group)
            .ThenInclude(g => g.GroupRoles)
            .ThenInclude(gr => gr.Role)
            .AsSplitQuery()
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        if (userWithGroups == null)
        {
            return new List<string>();
        }

        var directRoles = await _userManager.GetRolesAsync(user);

        var groupRoles = userWithGroups.UserGroups
            .SelectMany(ug => ug.Group.GroupRoles)
            .Select(gr => gr.Role.Name!)
            .Distinct();

        return directRoles.Union(groupRoles).ToList();
    }

    private async Task<string> GenerateAndSaveRefreshTokenAsync(Guid userId)
    {
        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            UserId = userId,
            ExpiryDate = DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:RefreshTokenLifetimeDays"]!))
        };

        await _refreshTokenRepository.AddAsync(refreshToken);
        return refreshToken.Token;
    }

    #endregion
}