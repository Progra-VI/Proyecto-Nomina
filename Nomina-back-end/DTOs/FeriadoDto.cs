using System.ComponentModel.DataAnnotations;

namespace Nomina_back_end.DTOs;

public class FeriadoDto
{
    public string? Id { get; set; }

    [Required, RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Formato yyyy-MM-dd.")]
    public string Fecha { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;
}