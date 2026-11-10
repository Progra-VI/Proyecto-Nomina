using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nomina_back_end.DTOs;
using Nomina_back_end.Entities;
using Nomina_back_end.Services;

namespace Nomina_back_end.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartamentosController : ControllerBase
{
    private readonly DepartamentoService _service;

    public DepartamentosController(DepartamentoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<DepartamentoRespuestaDto>>> ObtenerTodos()
    {
        var departamentos = await _service.ObtenerTodos();
        return Ok(departamentos.Select(ADto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DepartamentoRespuestaDto>> ObtenerPorId(string id)
    {
        var departamento = await _service.ObtenerPorId(id);
        if (departamento == null)
            return NotFound(new { mensaje = "Departamento no encontrado." });

        return Ok(ADto(departamento));
    }

    [HttpPost]
    [Authorize(Roles = Roles.RRHH)]
    public async Task<ActionResult<DepartamentoRespuestaDto>> Crear(DepartamentoGuardarDto dto)
    {
        var departamento = AEntidad(dto);
        var (ok, error) = await _service.Crear(departamento);
        if (!ok) return BadRequest(new { mensaje = error });

        return CreatedAtAction(nameof(ObtenerPorId), new { id = departamento.Id }, ADto(departamento));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.RRHH)]
    public async Task<IActionResult> Actualizar(string id, DepartamentoGuardarDto dto)
    {
        var (ok, error) = await _service.Actualizar(id, AEntidad(dto));
        if (!ok)
            return error == "Departamento no encontrado."
                ? NotFound(new { mensaje = error })
                : BadRequest(new { mensaje = error });

        return NoContent();
    }

    private static Departamento AEntidad(DepartamentoGuardarDto dto) => new()
    {
        Nombre = dto.Nombre,
        Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim()
    };

    private static DepartamentoRespuestaDto ADto(Departamento d) => new()
    {
        Id = d.Id,
        Nombre = d.Nombre,
        Descripcion = d.Descripcion
    };
}