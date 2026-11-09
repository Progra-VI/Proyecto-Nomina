using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nomina_back_end.DTOs;
using Nomina_back_end.Entities;

namespace Nomina_back_end.Services;

public class AuthService
{
    private readonly UsuarioService _usuarios;
    private readonly JwtSettings _jwt;

    public AuthService(UsuarioService usuarios, IOptions<JwtSettings> jwt)
    {
        _usuarios = usuarios;
        _jwt = jwt.Value;
    }

    public async Task<LoginRespuestaDto?> Login(LoginDto dto)
    {
        var usuario = await _usuarios.ObtenerPorCorreo(dto.Correo);

        // Mismo resultado (null) para usuario inexistente, inactivo o clave mala
        if (usuario == null || usuario.Estado != EstadosUsuario.Activo)
            return null;

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
            return null;

        var expiracion = DateTime.UtcNow.AddMinutes(_jwt.ExpiracionMinutos);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id!),
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim(ClaimTypes.Email, usuario.Correo),
            new Claim(ClaimTypes.Role, usuario.Rol)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expiracion,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new LoginRespuestaDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiracion = expiracion,
            Id = usuario.Id!,
            Nombre = usuario.Nombre,
            Rol = usuario.Rol
        };
    }
}