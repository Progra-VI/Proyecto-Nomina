using System.ComponentModel.DataAnnotations;

namespace Nomina_back_end.DTOs;

public class PermisoGuardarDto
{
    [Required]
    public string TipoPermisoId { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Formato yyyy-MM-dd.")]
    public string FechaInicio { get; set; } = string.Empty;

    [Required, RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Formato HH:mm.")]
    public string HoraInicio { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Formato yyyy-MM-dd.")]
    public string FechaFin { get; set; } = string.Empty;

    [Required, RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Formato HH:mm.")]
    public string HoraFin { get; set; } = string.Empty;

    [Required, StringLength(250, MinimumLength = 5)]
    public string Motivo { get; set; } = string.Empty;

    public bool ConGoce { get; set; }
}

public class ResolverSolicitudDto
{
    [StringLength(250)]
    public string? Observacion { get; set; }
}

public class PermisoRespuestaDto
{
    public string? Id { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public string NombreEmpleado { get; set; } = string.Empty;
    public string? FotoUrl { get; set; }
    public string TipoPermisoId { get; set; } = string.Empty;
    public string TipoPermiso { get; set; } = string.Empty;
    public string FechaInicio { get; set; } = string.Empty;
    public string HoraInicio { get; set; } = string.Empty;
    public string FechaFin { get; set; } = string.Empty;
    public string HoraFin { get; set; } = string.Empty;
    public string Motivo { get; set; } = string.Empty;
    public bool ConGoce { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? AprobadorId { get; set; }
    public string? AprobadorNombre { get; set; }
    public string? Observacion { get; set; }
    public DateTime? FechaResolucion { get; set; }
}