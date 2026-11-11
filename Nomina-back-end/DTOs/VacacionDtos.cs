using System.ComponentModel.DataAnnotations;

namespace Nomina_back_end.DTOs;

public class VacacionGuardarDto
{
    [Required, RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Formato yyyy-MM-dd.")]
    public string FechaInicio { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Formato yyyy-MM-dd.")]
    public string FechaFin { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Motivo { get; set; }
}

public class VacacionRespuestaDto
{
    public string? Id { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public string NombreEmpleado { get; set; } = string.Empty;
    public string? FotoUrl { get; set; }
    public string FechaInicio { get; set; } = string.Empty;
    public string FechaFin { get; set; } = string.Empty;
    public int Dias { get; set; }
    public string? Motivo { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? AprobadorId { get; set; }
    public string? AprobadorNombre { get; set; }
    public string? Observacion { get; set; }
    public DateTime? FechaResolucion { get; set; }
}

public class SaldoVacacionesDto
{
    public string UsuarioId { get; set; } = string.Empty;
    public string NombreEmpleado { get; set; } = string.Empty;
    public string FechaIngreso { get; set; } = string.Empty;
    public decimal Acumulado { get; set; }
    public decimal Disfrutado { get; set; }   // aprobadas
    public decimal Pendiente { get; set; }    // en espera de aprobación
    public decimal Saldo { get; set; }        // acumulado - disfrutado
    public decimal Disponible { get; set; }   // saldo - pendiente
    public List<VacacionRespuestaDto> Movimientos { get; set; } = new();
}