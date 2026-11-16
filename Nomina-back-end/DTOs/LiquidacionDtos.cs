using System.ComponentModel.DataAnnotations;

namespace Nomina_back_end.DTOs;

public class LiquidacionGuardarDto
{
    [Required]
    public string MotivoLiquidacionId { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "Formato yyyy-MM-dd.")]
    public string FechaSalida { get; set; } = string.Empty;
}

public class LiquidacionCrearDto : LiquidacionGuardarDto
{
    [Required]
    public string UsuarioId { get; set; } = string.Empty;
}

public class RubroDto
{
    public string Concepto { get; set; } = string.Empty;
    public string Detalle { get; set; } = string.Empty;
    public decimal Monto { get; set; }
}

public class LiquidacionDto
{
    public string? Id { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public string NombreEmpleado { get; set; } = string.Empty;
    public string Identificacion { get; set; } = string.Empty;
    public string Departamento { get; set; } = string.Empty;
    public string Puesto { get; set; } = string.Empty;
    public string FechaIngreso { get; set; } = string.Empty;
    public string FechaSalida { get; set; } = string.Empty;
    public string MotivoId { get; set; } = string.Empty;
    public string Motivo { get; set; } = string.Empty;
    public int AntiguedadMeses { get; set; }
    public string Antiguedad { get; set; } = string.Empty;
    public decimal SalarioBase { get; set; }
    public string Estado { get; set; } = string.Empty;
    public List<RubroDto> Ingresos { get; set; } = new();
    public List<RubroDto> Deducciones { get; set; } = new();
    public decimal TotalIngresos { get; set; }
    public decimal TotalDeducciones { get; set; }
    public decimal TotalNeto { get; set; }
    public bool TieneComprobante { get; set; }
}