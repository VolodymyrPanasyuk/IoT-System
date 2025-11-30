using IoT_System.Application.Common;
using IoT_System.Application.DTOs.Request;
using IoT_System.Application.DTOs.Response.Users;
using IoT_System.Application.Interfaces.Services.Auth;
using IoT_System.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IoT_System.Api.Controllers;

[Authorize]
[ApiController]
[Route($"{Constants.ApiRoutes.Identity}/[controller]")]
[ApiExplorerSettings(GroupName = Constants.SwaggerGroups.Identity)]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAccessValidationService _accessValidationService;

    public UsersController(IUserService userService, IAccessValidationService accessValidationService)
    {
        _userService = userService;
        _accessValidationService = accessValidationService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetAll([FromQuery] bool includeGroupsAndRoles = false)
    {
        var result = await _userService.GetAllAsync(includeGroupsAndRoles);
        return result.ToResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetById(Guid id, [FromQuery] bool includeGroupsAndRoles = false)
    {
        var result = await _userService.GetByIdAsync(id, includeGroupsAndRoles);
        return result.ToResult();
    }

    [HttpGet("username/{username}")]
    public async Task<ActionResult<UserResponse>> GetByUsername(string username, [FromQuery] bool includeGroupsAndRoles = false)
    {
        var result = await _userService.GetByUsernameAsync(username, includeGroupsAndRoles);
        return result.ToResult();
    }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create([FromBody] CreateUserRequest request)
    {
        if (request.RoleIds is { Count: > 0 })
        {
            var validationResult = await _accessValidationService.ValidateRolesAccessAsync(request.RoleIds);
            if (!validationResult.IsSuccess) return validationResult.ToResult();
        }

        var result = await _userService.CreateUserAsync(request);
        return result.ToResult();
    }

    [HttpPut]
    public async Task<ActionResult<UserResponse>> Update([FromBody] UpdateUserRequest request)
    {
        var validationResult = await _accessValidationService.ValidateUserAccessAsync(request.Id);
        if (!validationResult.IsSuccess) return validationResult.ToResult();

        var result = await _userService.UpdateUserAsync(request);
        return result.ToResult();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var validationResult = await _accessValidationService.ValidateUserAccessAsync(id);
        if (!validationResult.IsSuccess) return validationResult.ToResult();

        var result = await _userService.DeleteUserAsync(id);
        return result.ToResult();
    }
}