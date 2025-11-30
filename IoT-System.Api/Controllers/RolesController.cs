using IoT_System.Application.Common;
using IoT_System.Application.DTOs.Request;
using IoT_System.Application.DTOs.Response.Roles;
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
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly IAccessValidationService _accessValidationService;

    public RolesController(IRoleService roleService, IAccessValidationService accessValidationService)
    {
        _roleService = roleService;
        _accessValidationService = accessValidationService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleResponse>>> GetAll([FromQuery] bool includeUsersAndGroups = false)
    {
        var result = await _roleService.GetAllAsync(includeUsersAndGroups);
        return result.ToResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RoleResponse>> GetById(Guid id, [FromQuery] bool includeUsersAndGroups = false)
    {
        var result = await _roleService.GetByIdAsync(id, includeUsersAndGroups);
        return result.ToResult();
    }

    [HttpGet("name/{name}")]
    public async Task<ActionResult<RoleResponse>> GetByName(string name, [FromQuery] bool includeUsersAndGroups = false)
    {
        var result = await _roleService.GetByNameAsync(name, includeUsersAndGroups);
        return result.ToResult();
    }

    [HttpPost]
    public async Task<ActionResult<RoleResponse>> Create([FromBody] CreateRoleRequest request)
    {
        var result = await _roleService.CreateRoleAsync(request);
        return result.ToResult();
    }

    [HttpPut]
    public async Task<ActionResult<RoleResponse>> Update([FromBody] UpdateRoleRequest request)
    {
        var validationResult = await _accessValidationService.ValidateRolesAccessAsync([request.Id]);
        if (!validationResult.IsSuccess) return validationResult.ToResult();

        var result = await _roleService.UpdateRoleAsync(request);
        return result.ToResult();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var validationResult = await _accessValidationService.ValidateRolesAccessAsync([id]);
        if (!validationResult.IsSuccess) return validationResult.ToResult();

        var result = await _roleService.DeleteRoleAsync(id);
        return result.ToResult();
    }
}