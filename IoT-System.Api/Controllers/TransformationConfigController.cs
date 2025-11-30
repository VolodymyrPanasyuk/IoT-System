using IoT_System.Application.Common.Helpers;
using IoT_System.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IoT_System.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TransformationConfigController : ControllerBase
{
    [HttpGet]
    public ActionResult<List<TransformationConfigModel>> GetAllTransformationConfigs()
    {
        var configs = TransformationConfigHelper.GetAllTransformationConfigs();
        return Ok(configs);
    }
}