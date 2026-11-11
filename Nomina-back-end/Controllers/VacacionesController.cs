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
public class VacacionesController : ControllerBase
{
    private readonly VacacionService _service;

    public VacacionesController(VacacionService service)
    {
        _service = service;
    }

    private string UsuarioActualId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private string RolActual => User.FindFirstValue(ClaimTypes.Role)!;

    [HttpGet("saldo")]
    public async Task<ActionResult<SaldoVacacionesDto>> Saldo(string? usuarioId)
    {
        var objetivo = string.IsNullOrWhiteSpace(usuarioId) ? UsuarioActualId : usuarioId;

        if (!await _service.PuedeConsultar(UsuarioActualId, RolActual, objetivo))
            return Forbid();

        var (r, saldo) = await _service.ConsultarSaldo(objetivo);
        return r.Ok ? Ok(saldo) : StatusCode(r.Status, new { mensaje = r.Error });
    }

    [HttpGet]
    public async Task<ActionResult<List<VacacionRespuestaDto>>> Listar(
        string? usuarioId, string? estado, string? desde, string? hasta) =>
        Ok(await _service.Listar(UsuarioActualId, RolActual, usuarioId, estado, desde, hasta));

    [HttpGet("{id}")]
    public async Task<ActionResult<VacacionRespuestaDto>> ObtenerPorId(string id)
    {
        var (r, v) = await _service.ObtenerPorId(id, UsuarioActualId, RolActual);
        return r.Ok ? Ok(v) : StatusCode(r.Status, new { mensaje = r.Error });
    }

    [HttpPost]
    public async Task<ActionResult<VacacionRespuestaDto>> Crear(VacacionGuardarDto dto)
    {
        var (r, v) = await _service.Crear(UsuarioActualId, dto);
        if (!r.Ok) return StatusCode(r.Status, new { mensaje = r.Error });
        return CreatedAtAction(nameof(ObtenerPorId), new { id = v!.Id }, v);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<VacacionRespuestaDto>> Editar(string id, VacacionGuardarDto dto)
    {
        var (r, v) = await _service.Editar(id, UsuarioActualId, dto);
        return r.Ok ? Ok(v) : StatusCode(r.Status, new { mensaje = r.Error });
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