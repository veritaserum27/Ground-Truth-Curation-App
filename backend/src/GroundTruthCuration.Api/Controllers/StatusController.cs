using System.Threading.Tasks;
using GroundTruthCuration.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GroundTruthCuration.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatusController : ControllerBase
{
    private readonly IStatusService _statusService;

    public StatusController(IStatusService statusService)
    {
        _statusService = statusService;
    }

    [HttpGet]
    public async Task<IActionResult> GetStatus()
    {
        var statuses = await _statusService.GetDatabaseStatusesAsync();
        return Ok(statuses);
    }
}
