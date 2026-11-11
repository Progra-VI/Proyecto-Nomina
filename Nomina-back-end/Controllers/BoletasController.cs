using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nomina_back_end.DTOs;
using Nomina_back_end.Services;

namespace Nomina_back_end.Controllers;

[ApiController]
[Route("api/boletas")]
[Authorize]
public class BoletasController : ControllerBase
{
    private readonly PlanillaService _service;

    public BoletasController(PlanillaService service)
    {
        _service = service;
    }

    private string UsuarioActualId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private string RolActual => User.FindFirstValue(ClaimTypes.Role)!;

    [HttpGet("mias")]
    public async Task<ActionResult<List<BoletaDto>>> Mias() =>
        Ok(await _service.BoletasMias(UsuarioActualId));

    [HttpGet("{id}")]
    public async Task<ActionResult<BoletaDto>> ObtenerPorId(string id)
    {
        var (r, b) = await _service.ObtenerBoleta(id, UsuarioActualId, RolActual);
        return r.Ok ? Ok(b) : StatusCode(r.Status, new { mensaje = r.Error });
    }
}