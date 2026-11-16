using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nomina_back_end.DTOs;
using Nomina_back_end.Entities;
using Nomina_back_end.Services;

namespace Nomina_back_end.Controllers;

[ApiController]
[Route("api/equipo")]
[Authorize(Roles = Roles.JefaturaORRHH)]
public class EquipoController : ControllerBase
{
    private readonly UsuarioService _service;

    public EquipoController(UsuarioService service)
    {
        _service = service;
    }

    // Las personas activas que tienen a este usuario como jefatura directa
    [HttpGet]
    public async Task<ActionResult<List<UsuarioRespuestaDto>>> MiEquipo()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var equipo = await _service.ObtenerEquipo(id);
        return Ok(equipo.Select(UsuarioMapper.ADto));
    }
}