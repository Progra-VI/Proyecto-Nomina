
using Microsoft.AspNetCore.Mvc;
using Nomina_back_end.DTOs;
using Nomina_back_end.Entities;
using Nomina_back_end.Services;

namespace Nomina_back_end.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AsistenciasController : ControllerBase
{
    private readonly AsistenciaService _service;

    public AsistenciasController(AsistenciaService service)
    {
        _service = service;
    }

    // Registrar entrada
    [HttpPost("entrada")]
    public async Task<ActionResult<AsistenciaRespuestaDto>> RegistrarEntrada(
        AsistenciaEntradaDto dto)
    {
        var (ok, error, asistencia) =
            await _service.RegistrarEntrada(dto.UsuarioId);

        if (!ok)
            return BadRequest(new { mensaje = error });

        return Ok(ADto(asistencia!));
    }

    // Registrar salida
    [HttpPost("salida")]
    public async Task<ActionResult<AsistenciaRespuestaDto>> RegistrarSalida(
        AsistenciaSalidaDto dto)
    {
        var (ok, error, asistencia) =
            await _service.RegistrarSalida(dto.AsistenciaId);

        if (!ok)
        {
            return error == "Registro de asistencia no encontrado."
                ? NotFound(new { mensaje = error })
                : BadRequest(new { mensaje = error });
        }

        return Ok(ADto(asistencia!));
    }

    // Consultar historial de un empleado
    [HttpGet("empleado/{usuarioId}")]
    public async Task<ActionResult<List<AsistenciaRespuestaDto>>>
        ObtenerPorUsuario(string usuarioId)
    {
        var asistencias = await _service.ObtenerPorUsuario(usuarioId);

        return Ok(asistencias.Select(ADto).ToList());
    }

    // Consultar resumen mensual
    [HttpGet("resumen/{usuarioId}/{anio}/{mes}")]
    public async Task<ActionResult<AsistenciaResumenMensualDto>>
        ObtenerResumenMensual(
            string usuarioId,
            int anio,
            int mes)
    {
        var (ok, error, resumen) =
            await _service.ObtenerResumenMensual(usuarioId, anio, mes);

        if (!ok)
        {
            return error == "Empleado no encontrado."
                ? NotFound(new { mensaje = error })
                : BadRequest(new { mensaje = error });
        }

        return Ok(resumen);
    }

    // Convertir entidad a DTO de respuesta
    private static AsistenciaRespuestaDto ADto(Asistencia a)
    {
        return new AsistenciaRespuestaDto
        {
            Id = a.Id,
            UsuarioId = a.UsuarioId,
            Fecha = a.Fecha,
            HoraEntrada = a.HoraEntrada,
            HoraSalida = a.HoraSalida,
            Estado = a.Estado,
            MinutosTardanza = a.MinutosTardanza,
            MinutosSalidaAnticipada = a.MinutosSalidaAnticipada,
            Observaciones = a.Observaciones,
            EsAjusteManual = a.EsAjusteManual,
            JustificacionAjuste = a.JustificacionAjuste,
            AjustadoPor = a.AjustadoPor,
            FechaAjuste = a.FechaAjuste
        };
    }
}
