using System.ComponentModel.DataAnnotations;

namespace Nomina_back_end.DTOs;

public class LoginDto
{
    [Required, EmailAddress] public string Correo { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

public class LoginRespuestaDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiracion { get; set; }
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}