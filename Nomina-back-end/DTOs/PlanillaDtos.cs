using System.ComponentModel.DataAnnotations;

namespace Nomina_back_end.DTOs;

// ----- Configuración de ley -----
public class CargaSocialDto
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;

    [Range(0, 100)]
    public decimal Porcentaje { get; set; }
}

public class TramoRentaDto
{
    [Range(0, double.MaxValue)]
    public decimal Desde { get; set; }

    public decimal? Hasta { get; set; }

    [Range(0, 100)]
    public decimal Porcentaje { get; set; }
}

public class ConfiguracionLeyDto
{
    public List<CargaSocialDto> CargasSociales { get; set; } = new();
    public List<TramoRentaDto> TramosRenta { get; set; } = new();
}

// ----- Planilla -----
public class PlanillaCrearDto
{
    [Range(2000, 2100)]
    public int Anio { get; set; }

    [Range(1, 12)]
    public int Mes { get; set; }

    [Range(1, 2)]
    public int? Quincena { get; set; }
}

public class PlanillaRespuestaDto
{
    public string? Id { get; set; }
    public int Anio { get; set; }
    public int Mes { get; set; }
    public int? Quincena { get; set; }
    public string Desde { get; set; } = string.Empty;
    public string Hasta { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public int Empleados { get; set; }
    public decimal TotalIngresos { get; set; }
    public decimal TotalDeducciones { get; set; }
    public decimal TotalNeto { get; set; }
    public DateTime? FechaCalculo { get; set; }
}

public class LineaCargaDto
{
    public string Nombre { get; set; } = string.Empty;
    public decimal Porcentaje { get; set; }
    public decimal Monto { get; set; }
}

public class BoletaDto
{
    public string? Id { get; set; }
    public string PlanillaId { get; set; } = string.Empty;
    public string EstadoPlanilla { get; set; } = string.Empty;
    public int Anio { get; set; }
    public int Mes { get; set; }
    public int? Quincena { get; set; }
    public string Desde { get; set; } = string.Empty;
    public string Hasta { get; set; } = string.Empty;

    public string UsuarioId { get; set; } = string.Empty;
    public string NombreEmpleado { get; set; } = string.Empty;
    public string Identificacion { get; set; } = string.Empty;
    public string Departamento { get; set; } = string.Empty;
    public string Puesto { get; set; } = string.Empty;

    public decimal SalarioBase { get; set; }
    public decimal DiasLaborados { get; set; }
    public decimal DiasVacaciones { get; set; }
    public int AusenciasInjustificadas { get; set; }
    public int AusenciasJustificadas { get; set; }
    public decimal HorasExtra { get; set; }

    public decimal IngresoSalario { get; set; }
    public decimal IngresoVacaciones { get; set; }
    public decimal IngresoHorasExtra { get; set; }
    public decimal TotalIngresos { get; set; }

    public decimal DeduccionAusencias { get; set; }
    public decimal DeduccionPermisos { get; set; }
    public List<LineaCargaDto> CargasSociales { get; set; } = new();
    public decimal TotalCargas { get; set; }
    public decimal ImpuestoRenta { get; set; }
    public decimal TotalDeducciones { get; set; }

    public decimal SalarioNeto { get; set; }
}

public class PlanillaDetalleDto
{
    public PlanillaRespuestaDto Planilla { get; set; } = new();
    public List<BoletaDto> Boletas { get; set; } = new();
}

public class ReporteDepartamentoDto
{
    public string DepartamentoId { get; set; } = string.Empty;
    public string Departamento { get; set; } = string.Empty;
    public int Empleados { get; set; }
    public decimal TotalIngresos { get; set; }
    public decimal TotalDeducciones { get; set; }
    public decimal TotalNeto { get; set; }
}

public class ReportePlanillaDto
{
    public PlanillaRespuestaDto Planilla { get; set; } = new();
    public List<ReporteDepartamentoDto> PorDepartamento { get; set; } = new();
}