using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nomina_back_end.DTOs;
using Nomina_back_end.Services;

namespace Nomina_back_end.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;

    public AuthController(AuthService auth)
    {
        _auth = auth;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginRespuestaDto>> Login(LoginDto dto)
    {
        var resultado = await _auth.Login(dto);
        if (resultado == null)
            return Unauthorized(new { mensaje = "Correo o contraseña incorrectos." });

        return Ok(resultado);
    }
}