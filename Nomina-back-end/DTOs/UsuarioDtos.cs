using System.ComponentModel.DataAnnotations;

namespace Nomina_back_end.DTOs;

public class HorarioDto
{
    [Required, RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Formato HH:mm.")]
    public string HoraEntrada { get; set; } = "08:00";

    [Required, RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Formato HH:mm.")]
    public string HoraSalida { get; set; } = "17:00";

    [Required, MinLength(1)]
    public List<int> DiasLaborales { get; set; } = new() { 1, 2, 3, 4, 5 };
}

public class UsuarioActualizarDto
{
    [Required] public string Identificacion { get; set; } = string.Empty;
    [Required] public string Nombre { get; set; } = string.Empty;
    [Required, EmailAddress] public string Correo { get; set; } = string.Empty;
    [Required] public string Rol { get; set; } = string.Empty;
    [Required] public string DepartamentoId { get; set; } = string.Empty;
    [Required] public string Puesto { get; set; } = string.Empty;
    public string? JefaturaDirectaId { get; set; }
    [Required] public DateTime FechaIngreso { get; set; }
    [Range(0.01, double.MaxValue, ErrorMessage = "El salario debe ser mayor a 0.")]
    public decimal SalarioBase { get; set; }
    [Required] public string TipoJornada { get; set; } = string.Empty;
    [Required] public HorarioDto Horario { get; set; } = new();
    public string? FotoUrl { get; set; }
}

public class UsuarioCrearDto : UsuarioActualizarDto
{
    [Required, MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string Password { get; set; } = string.Empty;
}

public class UsuarioRespuestaDto
{
    public string? Id { get; set; }
    public string Identificacion { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string DepartamentoId { get; set; } = string.Empty;
    public string Puesto { get; set; } = string.Empty;
    public string? JefaturaDirectaId { get; set; }
    public DateTime FechaIngreso { get; set; }
    public decimal SalarioBase { get; set; }
    public string TipoJornada { get; set; } = string.Empty;
    public HorarioDto Horario { get; set; } = new();
    public string? FotoUrl { get; set; }
    public string Estado { get; set; } = string.Empty;
}