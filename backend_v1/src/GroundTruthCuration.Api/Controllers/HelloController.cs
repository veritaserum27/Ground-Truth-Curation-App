using Microsoft.AspNetCore.Mvc;
using GroundTruthCuration.Core;

namespace GroundTruthCuration.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HelloController : ControllerBase
{
    private readonly IHello _helloService;

    public HelloController(IHello helloService)
    {
        _helloService = helloService;
    }

    [HttpGet("{name}")]
    public ActionResult<string> GetHello(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest("Name cannot be empty");
        }

        var result = _helloService.GetHelloMessage(name);
        return Ok(result);
    }

    [HttpGet]
    public ActionResult<string> GetHello()
    {
        var result = _helloService.GetHelloMessage("World");
        return Ok(result);
    }
}
