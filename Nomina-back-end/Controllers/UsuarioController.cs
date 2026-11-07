using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nomina_back_end.DTOs;
using Nomina_back_end.Entities;
using Nomina_back_end.Services;

namespace Nomina_back_end.Controllers;

[ApiController]
[Route("api/[controller]")]
// [Authorize(Roles = Roles.RRHH)]
public class UsuariosController : ControllerBase
{
    private readonly UsuarioService _service;

    public UsuariosController(UsuarioService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<UsuarioRespuestaDto>>> ObtenerTodos()
    {
        var usuarios = await _service.ObtenerTodos();
        return Ok(usuarios.Select(ADto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UsuarioRespuestaDto>> ObtenerPorId(string id)
    {
        var usuario = await _service.ObtenerPorId(id);
        if (usuario == null) return NotFound(new { mensaje = "Usuario no encontrado." });
        return Ok(ADto(usuario));
    }

    [HttpPost]
    public async Task<ActionResult<UsuarioRespuestaDto>> Crear(UsuarioCrearDto dto)
    {
        var usuario = AEntidad(dto);
        var (ok, error) = await _service.Crear(usuario, dto.Password);
        if (!ok) return BadRequest(new { mensaje = error });

        return CreatedAtAction(nameof(ObtenerPorId), new { id = usuario.Id }, ADto(usuario));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(string id, UsuarioActualizarDto dto)
    {
        var (ok, error) = await _service.Actualizar(id, AEntidad(dto));
        if (!ok)
            return error == "Usuario no encontrado."
                ? NotFound(new { mensaje = error })
                : BadRequest(new { mensaje = error });

        return NoContent();
    }

    [HttpPatch("{id}/inactivar")]
    public async Task<IActionResult> Inactivar(string id)
    {
        var (ok, error) = await _service.Inactivar(id);
        if (!ok) return NotFound(new { mensaje = error });
        return NoContent();
    }


    private static Usuario AEntidad(UsuarioActualizarDto dto) => new()
    {
        Identificacion = dto.Identificacion.Trim(),
        Nombre = dto.Nombre.Trim(),
        Correo = dto.Correo,
        Rol = dto.Rol,
        DepartamentoId = dto.DepartamentoId,
        Puesto = dto.Puesto.Trim(),
        JefaturaDirectaId = string.IsNullOrWhiteSpace(dto.JefaturaDirectaId) ? null : dto.JefaturaDirectaId,
        FechaIngreso = DateTime.SpecifyKind(dto.FechaIngreso.Date, DateTimeKind.Utc),
        SalarioBase = dto.SalarioBase,
        TipoJornada = dto.TipoJornada,
        FotoUrl = dto.FotoUrl,
        Horario = new Horario
        {
            HoraEntrada = dto.Horario.HoraEntrada,
            HoraSalida = dto.Horario.HoraSalida,
            DiasLaborales = dto.Horario.DiasLaborales
        }
    };

    private static UsuarioRespuestaDto ADto(Usuario u) => new()
    {
        Id = u.Id,
        Identificacion = u.Identificacion,
        Nombre = u.Nombre,
        Correo = u.Correo,
        Rol = u.Rol,
        DepartamentoId = u.DepartamentoId,
        Puesto = u.Puesto,
        JefaturaDirectaId = u.JefaturaDirectaId,
        FechaIngreso = u.FechaIngreso,
        SalarioBase = u.SalarioBase,
        TipoJornada = u.TipoJornada,
        FotoUrl = u.FotoUrl,
        Estado = u.Estado,
        Horario = new HorarioDto
        {
            HoraEntrada = u.Horario.HoraEntrada,
            HoraSalida = u.Horario.HoraSalida,
            DiasLaborales = u.Horario.DiasLaborales
        }
    };
}