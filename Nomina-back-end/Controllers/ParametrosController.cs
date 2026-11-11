using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nomina_back_end.DTOs;
using Nomina_back_end.Entities;
using Nomina_back_end.Services;

namespace Nomina_back_end.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.RRHH)]
public class ParametrosController : ControllerBase
{
    private readonly ParametroService _service;

    public ParametrosController(ParametroService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<ParametroDto>>> Listar() =>
        Ok((await _service.ObtenerTodos()).Select(p => new ParametroDto
        {
            Clave = p.Clave,
            Valor = p.Valor,
            Descripcion = p.Descripcion
        }));

    // Crea el parámetro si no existe, o lo actualiza
    [HttpPut("{clave}")]
    public async Task<IActionResult> Guardar(string clave, ParametroGuardarDto dto)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(clave, @"^[a-z0-9_.]{3,80}$"))
            return BadRequest(new { mensaje = "Clave inválida (minúsculas, números, punto y guion bajo)." });

        await _service.Guardar(clave, dto.Valor, dto.Descripcion);
        return NoContent();
    }
}