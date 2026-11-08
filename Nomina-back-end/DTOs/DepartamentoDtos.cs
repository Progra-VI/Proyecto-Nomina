
using System.ComponentModel.DataAnnotations;

namespace Nomina_back_end.DTOs;

public class DepartamentoCrearDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, MinimumLength = 2,
        ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(300,
        ErrorMessage = "La descripción no puede superar los 300 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;
}

public class DepartamentoActualizarDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, MinimumLength = 2,
        ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(300,
        ErrorMessage = "La descripción no puede superar los 300 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;
}

public class DepartamentoRespuestaDto
{
    public string? Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }
}
