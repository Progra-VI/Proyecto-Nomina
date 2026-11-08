
using System.ComponentModel.DataAnnotations;

namespace Nomina_back_end.DTOs;

public class AsistenciaEntradaDto
{
    [Required(ErrorMessage = "El identificador del empleado es obligatorio.")]
    public string UsuarioId { get; set; } = string.Empty;
}

public class AsistenciaSalidaDto
{
    [Required(ErrorMessage = "El identificador de asistencia es obligatorio.")]
    public string AsistenciaId { get; set; } = string.Empty;
}

public class AsistenciaAjusteDto
{
    [Required(ErrorMessage = "La justificación es obligatoria.")]
    [StringLength(500, MinimumLength = 5,
        ErrorMessage = "La justificación debe tener entre 5 y 500 caracteres.")]
    public string Justificacion { get; set; } = string.Empty;

    public DateTime? HoraEntrada { get; set; }

    public DateTime? HoraSalida { get; set; }

    [StringLength(300)]
    public string? Observaciones { get; set; }
}

public class AsistenciaRespuestaDto
{
    public string? Id { get; set; }

    public string UsuarioId { get; set; } = string.Empty;

    public DateTime Fecha { get; set; }

    public DateTime? HoraEntrada { get; set; }

    public DateTime? HoraSalida { get; set; }

    public string Estado { get; set; } = string.Empty;

    public int MinutosTardanza { get; set; }

    public int MinutosSalidaAnticipada { get; set; }

    public string? Observaciones { get; set; }

    public bool EsAjusteManual { get; set; }

    public string? JustificacionAjuste { get; set; }

    public string? AjustadoPor { get; set; }

    public DateTime? FechaAjuste { get; set; }
}

public class AsistenciaResumenMensualDto
{
    public string UsuarioId { get; set; } = string.Empty;

    public int Anio { get; set; }

    public int Mes { get; set; }

    public int DiasRegistrados { get; set; }

    public int DiasCompletos { get; set; }

    public int Tardanzas { get; set; }

    public int Ausencias { get; set; }

    public int SalidasAnticipadas { get; set; }

    public int TotalMinutosTardanza { get; set; }
}
