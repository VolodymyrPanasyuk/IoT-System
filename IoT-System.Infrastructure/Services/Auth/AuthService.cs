using IoT_System.Application.Common;
using IoT_System.Application.DTOs.Request;
using IoT_System.Application.DTOs.Response.Auth;
using IoT_System.Application.Interfaces.Repositories.Auth;
using IoT_System.Application.Interfaces.Services.Auth;
using IoT_System.Application.Models;
using IoT_System.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using static IoT_System.Application.Common.Helpers.ExecutionHelper;

namespace IoT_System.Infrastructure.Services.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly JwtService _jwtService;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<User> userManager,
        IRefreshTokenRepository refreshTokenRepository,
        IGroupRepository groupRepository,
        IRoleRepository roleRepository,
        JwtService jwtService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _refreshTokenRepository = refreshTokenRepository;
        _groupRepository = groupRepository;
        _roleRepository = roleRepository;
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

        var userRolesResult = await GetUserRolesAsync(user);
        if (!userRolesResult.IsSuccess) return userRolesResult.ToOperationResult<(AuthResponse, string)>();

        var userGroupsResult = await GetUserGroupsAsync(user);
        if (!userGroupsResult.IsSuccess) return userGroupsResult.ToOperationResult<(AuthResponse, string)>();

        var accessToken = _jwtService.GenerateAccessToken(user, userRolesResult, userGroupsResult);

        var refreshTokenResult = await GenerateAndSaveRefreshTokenAsync(user.Id);
        if (!refreshTokenResult.IsSuccess) return refreshTokenResult.ToOperationResult<(AuthResponse, string)>();

        var authResponse = new AuthResponse
        {
            AccessToken = accessToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:AccessTokenLifetimeMinutes"]!))
        };

        return OperationResult<(AuthResponse, string)>.Success((authResponse, refreshTokenResult.Data!));
    }

    public async Task<OperationResult<(AuthResponse response, string refreshToken)>> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByNameAsync(request.UserName);
        if (existingUser != null)
        {
            return OperationResult<(AuthResponse, string)>.Conflict("User with this username already exists");
        }

        var errors = new List<string>();
        var user = new User
        {
            UserName = request.UserName,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var createUserResult = await _userManager.CreateAsync(user, request.Password);
        if (!createUserResult.Succeeded)
        {
            errors.AddRange(createUserResult.Errors.Select(e => e.Description));
            return OperationResult<(AuthResponse, string)>.BadRequest(errors);
        }

        var addToRoleResult = await _userManager.AddToRoleAsync(user, Constants.Roles.Viewer);
        if (!addToRoleResult.Succeeded)
        {
            errors.AddRange(addToRoleResult.Errors.Select(e => e.Description));
        }

        var userRolesResult = await GetUserRolesAsync(user);
        if (!userRolesResult.IsSuccess) return userRolesResult.ToOperationResult<(AuthResponse, string)>();

        var userGroupsResult = await GetUserGroupsAsync(user);
        if (!userGroupsResult.IsSuccess) return userGroupsResult.ToOperationResult<(AuthResponse, string)>();

        var accessToken = _jwtService.GenerateAccessToken(user, userRolesResult, userGroupsResult);

        var refreshTokenResult = await GenerateAndSaveRefreshTokenAsync(user.Id);
        if (!refreshTokenResult.IsSuccess) return refreshTokenResult.ToOperationResult<(AuthResponse, string)>();

        var authResponse = new AuthResponse
        {
            AccessToken = accessToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:AccessTokenLifetimeMinutes"]!))
        };

        var result = OperationResult<(AuthResponse, string)>.Created((authResponse, refreshTokenResult.Data!));
        if (errors.Any()) result.Errors = errors;
        return result;
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
            var deleteTokenResult = await _refreshTokenRepository.DeleteAsync(token);
            if (!deleteTokenResult.IsSuccess) return deleteTokenResult.ToOperationResult<(AuthResponse, string)>();

            return OperationResult<(AuthResponse, string)>.Unauthorized("Refresh token has expired");
        }

        var user = await _userManager.FindByIdAsync(token.UserId.ToString());
        if (user == null)
        {
            return OperationResult<(AuthResponse, string)>.NotFound("User not found");
        }

        await _refreshTokenRepository.DeleteAsync(token);

        var userRolesResult = await GetUserRolesAsync(user);
        if (!userRolesResult.IsSuccess) return userRolesResult.ToOperationResult<(AuthResponse, string)>();

        var userGroupsResult = await GetUserGroupsAsync(user);
        if (!userGroupsResult.IsSuccess) return userGroupsResult.ToOperationResult<(AuthResponse, string)>();

        var newAccessToken = _jwtService.GenerateAccessToken(user, userRolesResult, userGroupsResult);

        var newRefreshToken = await GenerateAndSaveRefreshTokenAsync(user.Id);
        if (!newRefreshToken.IsSuccess) return newRefreshToken.ToOperationResult<(AuthResponse, string)>();

        var authResponse = new AuthResponse
        {
            AccessToken = newAccessToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:AccessTokenLifetimeMinutes"]!))
        };

        return OperationResult<(AuthResponse, string)>.Success((authResponse, newRefreshToken.Data!));
    }

    #region Private Methods

    private Task<OperationResult<List<Role>>> GetUserRolesAsync(User user)
    {
        return ExecuteAsync(() =>
            _roleRepository.AsQueryable()
                .Where(r => r.UserRoles.Any(ur => ur.UserId == user.Id))
                .ToListAsync()
        );
    }

    private Task<OperationResult<List<Group>>> GetUserGroupsAsync(User user)
    {
        return ExecuteAsync(() =>
            _groupRepository.AsQueryable()
                .Include(g => g.GroupRoles)
                .ThenInclude(gr => gr.Role)
                .AsSplitQuery()
                .Where(g => g.UserGroups.Any(ug => ug.UserId == user.Id))
                .ToListAsync()
        );
    }

    private async Task<OperationResult<string>> GenerateAndSaveRefreshTokenAsync(Guid userId)
    {
        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            UserId = userId,
            ExpiryDate = DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:RefreshTokenLifetimeDays"]!))
        };

        var result = await _refreshTokenRepository.AddAsync(refreshToken);
        if (!result.IsSuccess) return result.ToOperationResult<string>();

        return refreshToken.Token;
    }

    #endregion
}