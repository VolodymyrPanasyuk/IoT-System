using IoT_System.Application.Common;
using IoT_System.Application.Common.Helpers;
using IoT_System.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IoT_System.Api.Controllers;

[Authorize]
[ApiController]
[Route($"{Constants.ApiRoutes.System}/[controller]")]
[ApiExplorerSettings(GroupName = Constants.SwaggerGroups.System)]
[Produces("application/json")]
public class TransformationConfigController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public ActionResult<List<TransformationConfigModel>> GetAllTransformationConfigs()
    {
        var configs = TransformationConfigHelper.GetAllTransformationConfigs();
        return Ok(configs);
    }
}