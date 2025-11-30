using IoT_System.Application.DTOs.Request;
using IoT_System.Application.DTOs.Response.Groups;
using IoT_System.Application.Interfaces.Services;
using IoT_System.Application.Interfaces.Services.Auth;
using IoT_System.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IoT_System.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;
    private readonly IAccessValidationService _accessValidationService;

    public GroupsController(IGroupService groupService, IAccessValidationService accessValidationService)
    {
        _groupService = groupService;
        _accessValidationService = accessValidationService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GroupResponse>>> GetAll([FromQuery] bool includeUsersAndRoles = false)
    {
        var result = await _groupService.GetAllAsync(includeUsersAndRoles);
        return result.ToResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GroupResponse>> GetById(Guid id, [FromQuery] bool includeUsersAndRoles = false)
    {
        var result = await _groupService.GetByIdAsync(id, includeUsersAndRoles);
        return result.ToResult();
    }

    [HttpPost]
    public async Task<ActionResult<GroupResponse>> Create([FromBody] CreateGroupRequest request)
    {
        if (request.RoleIds is { Count: > 0 })
        {
            var validationResult = await _accessValidationService.ValidateRolesAccessAsync(request.RoleIds);
            if (!validationResult.IsSuccess) return validationResult.ToResult();
        }

        if (request.UserIds is { Count: > 0 })
        {
            var validationResult = await _accessValidationService.ValidateUsersAccessAsync(request.UserIds);
            if (!validationResult.IsSuccess) return validationResult.ToResult();
        }

        var result = await _groupService.CreateGroupAsync(request);
        return result.ToResult();
    }

    [HttpPut]
    public async Task<ActionResult<GroupResponse>> Update(Guid id, [FromBody] UpdateGroupRequest request)
    {
        var validationResult = await _accessValidationService.ValidateGroupAccessAsync(id);
        if (!validationResult.IsSuccess) return validationResult.ToResult();

        var result = await _groupService.UpdateGroupAsync(request);
        return result.ToResult();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var validationResult = await _accessValidationService.ValidateGroupAccessAsync(id);
        if (!validationResult.IsSuccess) return validationResult.ToResult();

        var result = await _groupService.DeleteGroupAsync(id);
        return result.ToResult();
    }
}