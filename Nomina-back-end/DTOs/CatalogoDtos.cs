using System.ComponentModel.DataAnnotations;

namespace Nomina_back_end.DTOs;

public class TipoPermisoDto
{
    public string? Id { get; set; }

    [Required, StringLength(100, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Descripcion { get; set; }
}

public class TipoHoraExtraDto
{
    public string? Id { get; set; }

    [Required, StringLength(100, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Descripcion { get; set; }

    [Range(1.0, 5.0, ErrorMessage = "El recargo debe estar entre 1 y 5.")]
    public decimal Recargo { get; set; }
}

public class MotivoLiquidacionDto
{
    public string? Id { get; set; }

    [Required, StringLength(100, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Descripcion { get; set; }

    public bool PagaPreaviso { get; set; }
    public bool PagaCesantia { get; set; }
}