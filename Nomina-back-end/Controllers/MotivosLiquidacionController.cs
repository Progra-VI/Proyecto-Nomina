using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nomina_back_end.DTOs;
using Nomina_back_end.Entities;
using Nomina_back_end.Services;

namespace Nomina_back_end.Controllers;

[ApiController]
[Route("api/motivos-liquidacion")]
[Authorize]
public class MotivosLiquidacionController : ControllerBase
{
    private readonly MotivoLiquidacionService _service;

    public MotivosLiquidacionController(MotivoLiquidacionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<MotivoLiquidacionDto>>> ObtenerTodos() =>
        Ok((await _service.ObtenerTodos()).Select(ADto));

    [HttpGet("{id}")]
    public async Task<ActionResult<MotivoLiquidacionDto>> ObtenerPorId(string id)
    {
        var e = await _service.ObtenerPorId(id);
        return e == null ? NotFound(new { mensaje = "Registro no encontrado." }) : Ok(ADto(e));
    }

    [HttpPost]
    [Authorize(Roles = Roles.RRHH)]
    public async Task<ActionResult<MotivoLiquidacionDto>> Crear(MotivoLiquidacionDto dto)
    {
        var e = AEntidad(dto);
        var (ok, error) = await _service.Crear(e);
        if (!ok) return BadRequest(new { mensaje = error });
        return CreatedAtAction(nameof(ObtenerPorId), new { id = e.Id }, ADto(e));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.RRHH)]
    public async Task<IActionResult> Actualizar(string id, MotivoLiquidacionDto dto)
    {
        var (ok, error) = await _service.Actualizar(id, AEntidad(dto));
        if (!ok)
            return error == "Registro no encontrado."
                ? NotFound(new { mensaje = error })
                : BadRequest(new { mensaje = error });
        return NoContent();
    }

    private static MotivoLiquidacion AEntidad(MotivoLiquidacionDto d) => new()
    {
        Nombre = d.Nombre,
        Descripcion = string.IsNullOrWhiteSpace(d.Descripcion) ? null : d.Descripcion.Trim(),
        PagaPreaviso = d.PagaPreaviso,
        PagaCesantia = d.PagaCesantia
    };

    private static MotivoLiquidacionDto ADto(MotivoLiquidacion e) => new()
    {
        Id = e.Id,
        Nombre = e.Nombre,
        Descripcion = e.Descripcion,
        PagaPreaviso = e.PagaPreaviso,
        PagaCesantia = e.PagaCesantia
    };
}