using System.ComponentModel.DataAnnotations;

namespace Nomina_back_end.DTOs;

public class DepartamentoGuardarDto
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Descripcion { get; set; }
}

public class DepartamentoRespuestaDto
{
    public string? Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}