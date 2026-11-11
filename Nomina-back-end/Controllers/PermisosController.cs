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
public class PermisosController : ControllerBase
{
    private readonly PermisoService _service;

    public PermisosController(PermisoService service)
    {
        _service = service;
    }

    private string UsuarioActualId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private string RolActual => User.FindFirstValue(ClaimTypes.Role)!;

    [HttpGet]
    public async Task<ActionResult<List<PermisoRespuestaDto>>> Listar(
        string? usuarioId, string? estado, string? tipoId, string? desde, string? hasta) =>
        Ok(await _service.Listar(UsuarioActualId, RolActual, usuarioId, estado, tipoId, desde, hasta));

    [HttpGet("{id}")]
    public async Task<ActionResult<PermisoRespuestaDto>> ObtenerPorId(string id)
    {
        var (r, permiso) = await _service.ObtenerPorId(id, UsuarioActualId, RolActual);
        if (!r.Ok) return StatusCode(r.Status, new { mensaje = r.Error });
        return Ok(permiso);
    }

    [HttpPost]
    public async Task<ActionResult<PermisoRespuestaDto>> Crear(PermisoGuardarDto dto)
    {
        var (r, permiso) = await _service.Crear(UsuarioActualId, dto);
        if (!r.Ok) return StatusCode(r.Status, new { mensaje = r.Error });
        return CreatedAtAction(nameof(ObtenerPorId), new { id = permiso!.Id }, permiso);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PermisoRespuestaDto>> Editar(string id, PermisoGuardarDto dto)
    {
        var (r, permiso) = await _service.Editar(id, UsuarioActualId, dto);
        if (!r.Ok) return StatusCode(r.Status, new { mensaje = r.Error });
        return Ok(permiso);
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