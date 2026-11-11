using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nomina_back_end.DTOs;
using Nomina_back_end.Entities;
using Nomina_back_end.Services;

namespace Nomina_back_end.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FeriadosController : ControllerBase
{
    private readonly FeriadoService _service;

    public FeriadosController(FeriadoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<FeriadoDto>>> Listar(int? anio)
    {
        var a = anio ?? HoraCR.ALocal(DateTime.UtcNow).Year;
        return Ok((await _service.ObtenerPorAnio(a)).Select(ADto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FeriadoDto>> ObtenerPorId(string id)
    {
        var f = await _service.ObtenerPorId(id);
        return f == null ? NotFound(new { mensaje = "Feriado no encontrado." }) : Ok(ADto(f));
    }

    [HttpPost]
    [Authorize(Roles = Roles.RRHH)]
    public async Task<ActionResult<FeriadoDto>> Crear(FeriadoDto dto)
    {
        var f = AEntidad(dto);
        var (ok, error) = await _service.Crear(f);
        if (!ok) return BadRequest(new { mensaje = error });
        return CreatedAtAction(nameof(ObtenerPorId), new { id = f.Id }, ADto(f));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.RRHH)]
    public async Task<IActionResult> Actualizar(string id, FeriadoDto dto)
    {
        var (ok, error) = await _service.Actualizar(id, AEntidad(dto));
        if (!ok)
            return error == "Feriado no encontrado."
                ? NotFound(new { mensaje = error })
                : BadRequest(new { mensaje = error });
        return NoContent();
    }

    private static Feriado AEntidad(FeriadoDto d) => new() { Fecha = d.Fecha, Nombre = d.Nombre };

    private static FeriadoDto ADto(Feriado f) => new() { Id = f.Id, Fecha = f.Fecha, Nombre = f.Nombre };
}