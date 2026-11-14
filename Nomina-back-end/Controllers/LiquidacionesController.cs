using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nomina_back_end.DTOs;
using Nomina_back_end.Entities;
using Nomina_back_end.Services;

namespace Nomina_back_end.Controllers;

[ApiController]
[Route("api/liquidaciones")]
[Authorize(Roles = Roles.RRHH)]
public class LiquidacionesController : ControllerBase
{
    private readonly LiquidacionService _service;

    public LiquidacionesController(LiquidacionService service)
    {
        _service = service;
    }

    private string UsuarioActualId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<ActionResult<List<LiquidacionDto>>> Listar(string? estado, string? usuarioId) =>
        Ok(await _service.Listar(estado, usuarioId));

    [HttpGet("{id}")]
    public async Task<ActionResult<LiquidacionDto>> ObtenerPorId(string id)
    {
        var (r, l) = await _service.ObtenerPorId(id);
        return r.Ok ? Ok(l) : StatusCode(r.Status, new { mensaje = r.Error });
    }

    [HttpPost]
    public async Task<ActionResult<LiquidacionDto>> Crear(LiquidacionCrearDto dto)
    {
        var (r, l) = await _service.Crear(UsuarioActualId, dto);
        if (!r.Ok) return StatusCode(r.Status, new { mensaje = r.Error });
        return CreatedAtAction(nameof(ObtenerPorId), new { id = l!.Id }, l);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<LiquidacionDto>> Recalcular(string id, LiquidacionGuardarDto dto)
    {
        var (r, l) = await _service.Recalcular(id, UsuarioActualId, dto);
        return r.Ok ? Ok(l) : StatusCode(r.Status, new { mensaje = r.Error });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(string id)
    {
        var r = await _service.Eliminar(id);
        return r.Ok ? NoContent() : StatusCode(r.Status, new { mensaje = r.Error });
    }

    [HttpPatch("{id}/aprobar")]
    public async Task<IActionResult> Aprobar(string id)
    {
        var r = await _service.Aprobar(id, UsuarioActualId);
        return r.Ok ? NoContent() : StatusCode(r.Status, new { mensaje = r.Error });
    }

    [HttpPatch("{id}/pagar")]
    public async Task<IActionResult> Pagar(string id)
    {
        var r = await _service.Pagar(id, UsuarioActualId);
        return r.Ok ? NoContent() : StatusCode(r.Status, new { mensaje = r.Error });
    }

    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> Pdf(string id)
    {
        var (r, pdf, nombre) = await _service.ObtenerPdf(id);
        if (!r.Ok) return StatusCode(r.Status, new { mensaje = r.Error });
        return File(pdf!, "application/pdf", nombre);
    }
}