using System.ComponentModel.DataAnnotations;

namespace Nomina_back_end.DTOs;

public class ParametroGuardarDto
{
    [Range(0, 1000000)]
    public decimal Valor { get; set; }

    [StringLength(250)]
    public string? Descripcion { get; set; }
}

public class ParametroDto
{
    public string Clave { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public string? Descripcion { get; set; }
}