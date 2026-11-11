using System.ComponentModel.DataAnnotations;

namespace Nomina_back_end.DTOs;

public class HoraExtraGuardarDto
{
    [Required]
    public string TipoHoraExtraId { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Formato yyyy-MM-dd.")]
    public string Fecha { get; set; } = string.Empty;

    [Required, RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Formato HH:mm.")]
    public string HoraInicio { get; set; } = string.Empty;

    [Required, RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Formato HH:mm.")]
    public string HoraFin { get; set; } = string.Empty;

    [Required, StringLength(250, MinimumLength = 5)]
    public string Justificacion { get; set; } = string.Empty;
}

public class HoraExtraRespuestaDto
{
    public string? Id { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public string NombreEmpleado { get; set; } = string.Empty;
    public string? FotoUrl { get; set; }
    public string TipoHoraExtraId { get; set; } = string.Empty;
    public string TipoHoraExtra { get; set; } = string.Empty;
    public string Fecha { get; set; } = string.Empty;
    public string HoraInicio { get; set; } = string.Empty;
    public string HoraFin { get; set; } = string.Empty;
    public decimal Horas { get; set; }
    public string Justificacion { get; set; } = string.Empty;
    public decimal ValorHora { get; set; }
    public decimal Recargo { get; set; }
    public decimal Monto { get; set; }   // estimado hasta que se apruebe
    public string Estado { get; set; } = string.Empty;
    public string? AprobadorId { get; set; }
    public string? AprobadorNombre { get; set; }
    public string? Observacion { get; set; }
    public DateTime? FechaResolucion { get; set; }
}

public class ReporteLineaDto
{
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal Horas { get; set; }
    public decimal Monto { get; set; }
    public int Solicitudes { get; set; }
}

public class ReporteHorasExtraDto
{
    public string? Desde { get; set; }
    public string? Hasta { get; set; }
    public decimal TotalHoras { get; set; }
    public decimal TotalMonto { get; set; }
    public List<ReporteLineaDto> PorEmpleado { get; set; } = new();
    public List<ReporteLineaDto> PorDepartamento { get; set; } = new();
}