using IoT_System.Application.DTOs.Request;
using IoT_System.Application.DTOs.Response.Roles;
using IoT_System.Application.Interfaces.Services;
using IoT_System.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IoT_System.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
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
        var result = await _roleService.UpdateRoleAsync(request);
        return result.ToResult();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _roleService.DeleteRoleAsync(id);
        return result.ToResult();
    }
}