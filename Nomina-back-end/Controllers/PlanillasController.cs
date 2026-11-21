using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nomina_back_end.DTOs;
using Nomina_back_end.Entities;
using Nomina_back_end.Services;

namespace Nomina_back_end.Controllers;

[ApiController]
[Route("api/planillas")]
[Authorize(Roles = Roles.RRHH)]
public class PlanillasController : ControllerBase
{
    private readonly PlanillaService _service;

    public PlanillasController(PlanillaService service)
    {
        _service = service;
    }

    private string UsuarioActualId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<ActionResult<List<PlanillaRespuestaDto>>> Listar(int? anio) =>
        Ok(await _service.Listar(anio));

    [HttpGet("{id}")]
    public async Task<ActionResult<PlanillaDetalleDto>> Detalle(string id)
    {
        var (r, d) = await _service.ObtenerDetalle(id);
        return r.Ok ? Ok(d) : StatusCode(r.Status, new { mensaje = r.Error });
    }

    [HttpGet("{id}/reporte")]
    public async Task<ActionResult<ReportePlanillaDto>> Reporte(string id)
    {
        var (r, rep) = await _service.Reporte(id);
        return r.Ok ? Ok(rep) : StatusCode(r.Status, new { mensaje = r.Error });
    }

    [HttpPost]
    public async Task<ActionResult<PlanillaRespuestaDto>> Crear(PlanillaCrearDto dto)
    {
        var (r, p) = await _service.Crear(UsuarioActualId, dto);
        if (!r.Ok) return StatusCode(r.Status, new { mensaje = r.Error });
        return CreatedAtAction(nameof(Detalle), new { id = p!.Id }, p);
    }

    [HttpPost("{id}/calcular")]
    public async Task<ActionResult<PlanillaRespuestaDto>> Calcular(string id)
    {
        var (r, p) = await _service.Calcular(id, UsuarioActualId);
        return r.Ok ? Ok(p) : StatusCode(r.Status, new { mensaje = r.Error });
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

    [HttpGet("{id}/boletas/pdf")]
    public async Task<IActionResult> BoletasPdf(string id)
    {
        var (r, d) = await _service.ObtenerDetalle(id);
        if (!r.Ok) return StatusCode(r.Status, new { mensaje = r.Error });

        if (d!.Boletas.Count == 0)
            return BadRequest(new { mensaje = "La planilla aún no tiene boletas: calcúlala primero." });

        var pdf = BoletaPdf.Generar(d.Boletas);
        return File(pdf, "application/pdf", $"planilla-{d.Planilla.Desde}-{d.Planilla.Hasta}.pdf");
    }
}