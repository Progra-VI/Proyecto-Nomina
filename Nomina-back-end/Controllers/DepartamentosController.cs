
using Microsoft.AspNetCore.Mvc;
using Nomina_back_end.DTOs;
using Nomina_back_end.Entities;
using Nomina_back_end.Services;

namespace Nomina_back_end.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    public async Task<ActionResult<DepartamentoRespuestaDto>> Crear(
        DepartamentoCrearDto dto)
    {
        var departamento = new Departamento
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion
        };

        var (ok, error) = await _service.Crear(departamento);

        if (!ok)
            return BadRequest(new { mensaje = error });

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = departamento.Id },
            ADto(departamento)
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(
        string id,
        DepartamentoActualizarDto dto)
    {
        var departamento = new Departamento
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion
        };

        var (ok, error) = await _service.Actualizar(id, departamento);

        if (!ok)
        {
            return error == "Departamento no encontrado."
                ? NotFound(new { mensaje = error })
                : BadRequest(new { mensaje = error });
        }

        return NoContent();
    }

    [HttpPatch("{id}/inactivar")]
    public async Task<IActionResult> Inactivar(string id)
    {
        var (ok, error) = await _service.Inactivar(id);

        if (!ok)
        {
            return error == "Departamento no encontrado."
                ? NotFound(new { mensaje = error })
                : BadRequest(new { mensaje = error });
        }

        return NoContent();
    }

    private static DepartamentoRespuestaDto ADto(Departamento d)
    {
        return new DepartamentoRespuestaDto
        {
            Id = d.Id,
            Nombre = d.Nombre,
            Descripcion = d.Descripcion,
            Estado = d.Estado,
            FechaCreacion = d.FechaCreacion,
            FechaActualizacion = d.FechaActualizacion
        };
    }
}
