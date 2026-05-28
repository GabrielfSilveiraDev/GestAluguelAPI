using BackEndAluguel.Api.Background;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEndAluguel.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class SistemaController : ControllerBase
{
    private readonly HeartbeatEstado _estado;

    public SistemaController(HeartbeatEstado estado) => _estado = estado;

    [HttpPost("heartbeat")]
    public IActionResult Heartbeat()
    {
        _estado.Registrar();
        return Ok();
    }
}
