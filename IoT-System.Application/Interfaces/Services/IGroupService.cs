using IoT_System.Application.DTOs.Request;
using IoT_System.Application.DTOs.Response.Groups;
using IoT_System.Application.Models;

namespace IoT_System.Application.Interfaces.Services;

public interface IGroupService
{
    Task<OperationResult<GroupResponse>> CreateGroupAsync(CreateGroupRequest request);
    Task<OperationResult<GroupResponse>> UpdateGroupAsync(UpdateGroupRequest request);
    Task<OperationResult> DeleteGroupAsync(Guid id);
    Task<OperationResult<GroupResponse>> GetByIdAsync(Guid id);
    Task<OperationResult<IEnumerable<GroupResponse>>> GetAllAsync();
}