using System.ComponentModel.DataAnnotations;

namespace Nomina_back_end.DTOs;

public class AjusteAsistenciaDto
{
    [Required]
    public string UsuarioId { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Formato yyyy-MM-dd.")]
    public string Fecha { get; set; } = string.Empty;

    [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Formato HH:mm.")]
    public string? HoraEntrada { get; set; }

    [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Formato HH:mm.")]
    public string? HoraSalida { get; set; }

    [Required, StringLength(250, MinimumLength = 5)]
    public string Justificacion { get; set; } = string.Empty;
}

public class AsistenciaRespuestaDto
{
    public string? Id { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public string NombreEmpleado { get; set; } = string.Empty;
    public string Fecha { get; set; } = string.Empty;
    public string? HoraEntrada { get; set; }
    public string? HoraSalida { get; set; }
    public bool Tardia { get; set; }
    public int MinutosTardia { get; set; }
    public bool SalidaAnticipada { get; set; }
    public int MinutosSalidaAnticipada { get; set; }
    public decimal HorasTrabajadas { get; set; }
    public bool Ajustada { get; set; }
}

public class ResumenAsistenciaDto
{
    public string UsuarioId { get; set; } = string.Empty;
    public string NombreEmpleado { get; set; } = string.Empty;
    public int Anio { get; set; }
    public int Mes { get; set; }
    public int DiasLaborados { get; set; }
    public int Tardias { get; set; }
    public int SalidasAnticipadas { get; set; }
    public int AusenciasJustificadas { get; set; }
    public int AusenciasInjustificadas { get; set; }
    public decimal HorasTrabajadas { get; set; }
    public List<string> FechasAusencia { get; set; } = new();
}