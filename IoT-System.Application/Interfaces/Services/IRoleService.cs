using IoT_System.Application.DTOs.Request;
using IoT_System.Application.DTOs.Response.Roles;
using IoT_System.Application.Models;

namespace IoT_System.Application.Interfaces.Services;

public interface IRoleService
{
    Task<OperationResult<RoleResponse>> CreateRoleAsync(CreateRoleRequest request);
    Task<OperationResult<RoleResponse>> UpdateRoleAsync(UpdateRoleRequest request);
    Task<OperationResult> DeleteRoleAsync(Guid id);
    Task<OperationResult<RoleResponse>> GetByIdAsync(Guid id);
    Task<OperationResult<RoleResponse>> GetByNameAsync(string name);
    Task<OperationResult<IEnumerable<RoleResponse>>> GetAllAsync();
}