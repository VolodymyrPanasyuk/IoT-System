using IoT_System.Application.DTOs.Request;
using IoT_System.Application.DTOs.Response.Users;
using IoT_System.Application.Models;

namespace IoT_System.Application.Interfaces.Services.Auth;

public interface IUserService
{
    Task<OperationResult<UserResponse>> CreateUserAsync(CreateUserRequest request);
    Task<OperationResult<UserResponse>> UpdateUserAsync(UpdateUserRequest request);
    Task<OperationResult> DeleteUserAsync(Guid userId);
    Task<OperationResult<UserResponse>> GetByIdAsync(Guid id, bool includeGroupsAndRoles = false);
    Task<OperationResult<UserResponse>> GetByUsernameAsync(string username, bool includeGroupsAndRoles = false);
    Task<OperationResult<IEnumerable<UserResponse>>> GetAllAsync(bool includeGroupsAndRoles = false);
}