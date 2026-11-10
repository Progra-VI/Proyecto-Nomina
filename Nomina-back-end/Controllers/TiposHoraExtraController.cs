using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nomina_back_end.DTOs;
using Nomina_back_end.Entities;
using Nomina_back_end.Services;

namespace Nomina_back_end.Controllers;

[ApiController]
[Route("api/tipos-hora-extra")]
[Authorize]
public class TiposHoraExtraController : ControllerBase
{
    private readonly TipoHoraExtraService _service;

    public TiposHoraExtraController(TipoHoraExtraService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<TipoHoraExtraDto>>> ObtenerTodos() =>
        Ok((await _service.ObtenerTodos()).Select(ADto));

    [HttpGet("{id}")]
    public async Task<ActionResult<TipoHoraExtraDto>> ObtenerPorId(string id)
    {
        var e = await _service.ObtenerPorId(id);
        return e == null ? NotFound(new { mensaje = "Registro no encontrado." }) : Ok(ADto(e));
    }

    [HttpPost]
    [Authorize(Roles = Roles.RRHH)]
    public async Task<ActionResult<TipoHoraExtraDto>> Crear(TipoHoraExtraDto dto)
    {
        var e = AEntidad(dto);
        var (ok, error) = await _service.Crear(e);
        if (!ok) return BadRequest(new { mensaje = error });
        return CreatedAtAction(nameof(ObtenerPorId), new { id = e.Id }, ADto(e));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.RRHH)]
    public async Task<IActionResult> Actualizar(string id, TipoHoraExtraDto dto)
    {
        var (ok, error) = await _service.Actualizar(id, AEntidad(dto));
        if (!ok)
            return error == "Registro no encontrado."
                ? NotFound(new { mensaje = error })
                : BadRequest(new { mensaje = error });
        return NoContent();
    }

    private static TipoHoraExtra AEntidad(TipoHoraExtraDto d) => new()
    {
        Nombre = d.Nombre,
        Descripcion = string.IsNullOrWhiteSpace(d.Descripcion) ? null : d.Descripcion.Trim(),
        Recargo = d.Recargo
    };

    private static TipoHoraExtraDto ADto(TipoHoraExtra e) => new()
    {
        Id = e.Id,
        Nombre = e.Nombre,
        Descripcion = e.Descripcion,
        Recargo = e.Recargo
    };
}