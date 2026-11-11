using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nomina_back_end.DTOs;
using Nomina_back_end.Entities;
using Nomina_back_end.Services;

namespace Nomina_back_end.Controllers;

[ApiController]
[Route("api/horas-extras")]
[Authorize]
public class HorasExtrasController : ControllerBase
{
    private readonly HoraExtraService _service;

    public HorasExtrasController(HoraExtraService service)
    {
        _service = service;
    }

    private string UsuarioActualId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private string RolActual => User.FindFirstValue(ClaimTypes.Role)!;

    [HttpGet]
    public async Task<ActionResult<List<HoraExtraRespuestaDto>>> Listar(
        string? usuarioId, string? departamentoId, string? estado, string? tipoId, string? desde, string? hasta) =>
        Ok(await _service.Listar(UsuarioActualId, RolActual, usuarioId, departamentoId, estado, tipoId, desde, hasta));

    [HttpGet("reporte")]
    [Authorize(Roles = Roles.JefaturaORRHH)]
    public async Task<ActionResult<ReporteHorasExtraDto>> Reporte(
        string? usuarioId, string? departamentoId, string? desde, string? hasta) =>
        Ok(await _service.Reporte(UsuarioActualId, RolActual, usuarioId, departamentoId, desde, hasta));

    [HttpGet("{id}")]
    public async Task<ActionResult<HoraExtraRespuestaDto>> ObtenerPorId(string id)
    {
        var (r, h) = await _service.ObtenerPorId(id, UsuarioActualId, RolActual);
        return r.Ok ? Ok(h) : StatusCode(r.Status, new { mensaje = r.Error });
    }

    [HttpPost]
    public async Task<ActionResult<HoraExtraRespuestaDto>> Crear(HoraExtraGuardarDto dto)
    {
        var (r, h) = await _service.Crear(UsuarioActualId, dto);
        if (!r.Ok) return StatusCode(r.Status, new { mensaje = r.Error });
        return CreatedAtAction(nameof(ObtenerPorId), new { id = h!.Id }, h);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<HoraExtraRespuestaDto>> Editar(string id, HoraExtraGuardarDto dto)
    {
        var (r, h) = await _service.Editar(id, UsuarioActualId, dto);
        return r.Ok ? Ok(h) : StatusCode(r.Status, new { mensaje = r.Error });
    }

    [HttpPatch("{id}/cancelar")]
    public async Task<IActionResult> Cancelar(string id)
    {
        var r = await _service.Cancelar(id, UsuarioActualId);
        return r.Ok ? NoContent() : StatusCode(r.Status, new { mensaje = r.Error });
    }

    [HttpPatch("{id}/aprobar")]
    [Authorize(Roles = Roles.Jefatura)]
    public async Task<IActionResult> Aprobar(string id, ResolverSolicitudDto? dto)
    {
        var r = await _service.Resolver(id, UsuarioActualId, EstadosSolicitud.Aprobada, dto?.Observacion);
        return r.Ok ? NoContent() : StatusCode(r.Status, new { mensaje = r.Error });
    }

    [HttpPatch("{id}/rechazar")]
    [Authorize(Roles = Roles.Jefatura)]
    public async Task<IActionResult> Rechazar(string id, ResolverSolicitudDto? dto)
    {
        var r = await _service.Resolver(id, UsuarioActualId, EstadosSolicitud.Rechazada, dto?.Observacion);
        return r.Ok ? NoContent() : StatusCode(r.Status, new { mensaje = r.Error });
    }
}