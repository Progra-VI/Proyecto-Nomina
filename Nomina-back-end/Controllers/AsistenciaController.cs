using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nomina_back_end.DTOs;
using Nomina_back_end.Entities;
using Nomina_back_end.Services;

namespace Nomina_back_end.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AsistenciaController : ControllerBase
{
    private readonly AsistenciaService _service;

    public AsistenciaController(AsistenciaService service)
    {
        _service = service;
    }

    private string UsuarioActualId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private string RolActual => User.FindFirstValue(ClaimTypes.Role)!;

    [HttpPost("entrada")]
    public async Task<ActionResult<AsistenciaRespuestaDto>> Entrada()
    {
        var (a, error) = await _service.MarcarEntrada(UsuarioActualId);
        if (a == null) return BadRequest(new { mensaje = error });
        return Ok(ADto(a, string.Empty));
    }

    [HttpPost("salida")]
    public async Task<ActionResult<AsistenciaRespuestaDto>> Salida()
    {
        var (a, error) = await _service.MarcarSalida(UsuarioActualId);
        if (a == null) return BadRequest(new { mensaje = error });
        return Ok(ADto(a, string.Empty));
    }

    [HttpGet("mia")]
    public async Task<ActionResult<List<AsistenciaRespuestaDto>>> Mia(string? desde, string? hasta)
    {
        var lista = await _service.Historial(UsuarioActualId, desde, hasta);
        return Ok(lista.Select(x => ADto(x.asistencia, x.nombre)));
    }

    [HttpGet]
    [Authorize(Roles = Roles.JefaturaORRHH)]
    public async Task<ActionResult<List<AsistenciaRespuestaDto>>> Listar(
        string? usuarioId, string? departamentoId, string? desde, string? hasta)
    {
        var lista = await _service.Listar(UsuarioActualId, RolActual, usuarioId, departamentoId, desde, hasta);
        return Ok(lista.Select(x => ADto(x.asistencia, x.nombre)));
    }

    [HttpGet("resumen")]
    public async Task<ActionResult<ResumenAsistenciaDto>> Resumen(string? usuarioId, int? anio, int? mes)
    {
        var ahora = HoraCR.ALocal(DateTime.UtcNow);
        var a = anio ?? ahora.Year;
        var m = mes ?? ahora.Month;

        if (m < 1 || m > 12 || a < 2000 || a > 2100)
            return BadRequest(new { mensaje = "Año o mes inválido." });

        var objetivo = string.IsNullOrWhiteSpace(usuarioId) ? UsuarioActualId : usuarioId;

        if (!await _service.PuedeConsultar(UsuarioActualId, RolActual, objetivo))
            return Forbid();

        var resumen = await _service.Resumen(objetivo, a, m);
        return resumen == null
            ? NotFound(new { mensaje = "Empleado no encontrado." })
            : Ok(resumen);
    }

    [HttpPut("ajuste")]
    [Authorize(Roles = Roles.RRHH)]
    public async Task<IActionResult> Ajustar(AjusteAsistenciaDto dto)
    {
        var (ok, error) = await _service.Ajustar(UsuarioActualId, dto.UsuarioId, dto.Fecha,
            dto.HoraEntrada, dto.HoraSalida, dto.Justificacion);

        return ok ? NoContent() : BadRequest(new { mensaje = error });
    }

    private static AsistenciaRespuestaDto ADto(Asistencia a, string nombre) => new()
    {
        Id = a.Id,
        UsuarioId = a.UsuarioId,
        NombreEmpleado = nombre,
        Fecha = a.Fecha,
        HoraEntrada = a.HoraEntrada == null ? null : HoraCR.ALocal(a.HoraEntrada.Value).ToString("HH:mm"),
        HoraSalida = a.HoraSalida == null ? null : HoraCR.ALocal(a.HoraSalida.Value).ToString("HH:mm"),
        Tardia = a.Tardia,
        MinutosTardia = a.MinutosTardia,
        SalidaAnticipada = a.SalidaAnticipada,
        MinutosSalidaAnticipada = a.MinutosSalidaAnticipada,
        HorasTrabajadas = a.HorasTrabajadas,
        Ajustada = a.Ajustes.Count > 0
    };
}