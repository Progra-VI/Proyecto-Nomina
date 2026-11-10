using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nomina_back_end.DTOs;
using Nomina_back_end.Entities;
using Nomina_back_end.Services;

namespace Nomina_back_end.Controllers;

[ApiController]
[Route("api/tipos-permiso")]
[Authorize]
public class TiposPermisoController : ControllerBase
{
    private readonly TipoPermisoService _service;

    public TiposPermisoController(TipoPermisoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<TipoPermisoDto>>> ObtenerTodos() =>
        Ok((await _service.ObtenerTodos()).Select(ADto));

    [HttpGet("{id}")]
    public async Task<ActionResult<TipoPermisoDto>> ObtenerPorId(string id)
    {
        var e = await _service.ObtenerPorId(id);
        return e == null ? NotFound(new { mensaje = "Registro no encontrado." }) : Ok(ADto(e));
    }

    [HttpPost]
    [Authorize(Roles = Roles.RRHH)]
    public async Task<ActionResult<TipoPermisoDto>> Crear(TipoPermisoDto dto)
    {
        var e = AEntidad(dto);
        var (ok, error) = await _service.Crear(e);
        if (!ok) return BadRequest(new { mensaje = error });
        return CreatedAtAction(nameof(ObtenerPorId), new { id = e.Id }, ADto(e));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.RRHH)]
    public async Task<IActionResult> Actualizar(string id, TipoPermisoDto dto)
    {
        var (ok, error) = await _service.Actualizar(id, AEntidad(dto));
        if (!ok)
            return error == "Registro no encontrado."
                ? NotFound(new { mensaje = error })
                : BadRequest(new { mensaje = error });
        return NoContent();
    }

    private static TipoPermiso AEntidad(TipoPermisoDto d) => new()
    {
        Nombre = d.Nombre,
        Descripcion = string.IsNullOrWhiteSpace(d.Descripcion) ? null : d.Descripcion.Trim()
    };

    private static TipoPermisoDto ADto(TipoPermiso e) => new()
    {
        Id = e.Id,
        Nombre = e.Nombre,
        Descripcion = e.Descripcion
    };
}