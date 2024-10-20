#region

using Asp.Versioning;
using Innov8.Server.DbManager.InnovDb;
using Microsoft.AspNetCore.Mvc;

#endregion

namespace Innov8.Server.ApiService.Controllers;

[ApiVersion("1.0")]
[ApiVersion("2.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public abstract class ApiControllerBase(InnovDbContext context)
  : ControllerBase
{
  protected readonly InnovDbContext Context = context;

  [HttpGet("version")]
  [MapToApiVersion("1.0")]
  public IActionResult GetVersion()
  {
    var version = HttpContext.GetRequestedApiVersion();
    if (version != null)
    {
      return Ok(new { Version = version.ToString() });
    }

    return BadRequest("Version information is not available");
  }

  [HttpGet("version")]
  [MapToApiVersion("2.0")]
  public IActionResult GetVersionV2()
  {
    var version = HttpContext.GetRequestedApiVersion();
    if (version != null)
    {
      return Ok(new { Version = $"{version} (～￣▽￣)～" });
    }

    return Ok(new { Version = "2.0 (～￣▽￣)～" });
  }
}
