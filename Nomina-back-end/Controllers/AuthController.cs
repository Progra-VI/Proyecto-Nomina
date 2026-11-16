using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nomina_back_end.DTOs;
using Nomina_back_end.Entities;
using Nomina_back_end.Services;

namespace Nomina_back_end.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;
    private readonly UsuarioService _usuarios;

    public AuthController(AuthService auth, UsuarioService usuarios)
    {
        _auth = auth;
        _usuarios = usuarios;
    }

    private string UsuarioActualId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginRespuestaDto>> Login(LoginDto dto)
    {
        var resultado = await _auth.Login(dto);
        if (resultado == null)
            return Unauthorized(new { mensaje = "Correo o contraseña incorrectos." });

        return Ok(resultado);
    }

    // Datos del usuario con sesión iniciada: el frontend lo usa para el menú por rol
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UsuarioRespuestaDto>> Me()
    {
        var u = await _usuarios.ObtenerPorId(UsuarioActualId);
        if (u == null || u.Estado != EstadosUsuario.Activo) return Unauthorized();

        return Ok(UsuarioMapper.ADto(u));
    }

    [Authorize]
    [HttpPut("password")]
    public async Task<IActionResult> CambiarPassword(CambiarPasswordDto dto)
    {
        var r = await _usuarios.CambiarPassword(UsuarioActualId, dto.PasswordActual, dto.PasswordNueva);
        return r.Ok ? NoContent() : StatusCode(r.Status, new { mensaje = r.Error });
    }
}