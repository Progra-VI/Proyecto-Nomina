using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nomina_back_end.DTOs;
using Nomina_back_end.Entities;
using Nomina_back_end.Services;

namespace Nomina_back_end.Controllers;

[ApiController]
[Route("api/configuracion-ley")]
[Authorize(Roles = Roles.RRHH)]
public class ConfiguracionLeyController : ControllerBase
{
    private readonly ConfiguracionLeyService _service;

    public ConfiguracionLeyController(ConfiguracionLeyService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ConfiguracionLeyDto>> Obtener() => Ok(await _service.Obtener());

    [HttpPut]
    public async Task<IActionResult> Guardar(ConfiguracionLeyDto dto)
    {
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var r = await _service.Guardar(usuarioId, dto);
        return r.Ok ? NoContent() : StatusCode(r.Status, new { mensaje = r.Error });
    }
}