using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Nomina_back_end.Entities;

public class LineaCarga
{
    [BsonElement("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [BsonElement("porcentaje"), BsonRepresentation(BsonType.Decimal128)]
    public decimal Porcentaje { get; set; }

    [BsonElement("monto"), BsonRepresentation(BsonType.Decimal128)]
    public decimal Monto { get; set; }
}

public class BoletaPago
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("planilla_id"), BsonRepresentation(BsonType.ObjectId)]
    public string PlanillaId { get; set; } = string.Empty;

    [BsonElement("usuario_id"), BsonRepresentation(BsonType.ObjectId)]
    public string UsuarioId { get; set; } = string.Empty;

    // Copia de los datos del empleado al momento del cálculo
    [BsonElement("nombre_empleado")] public string NombreEmpleado { get; set; } = string.Empty;
    [BsonElement("identificacion")] public string Identificacion { get; set; } = string.Empty;
    [BsonElement("departamento_id")] public string DepartamentoId { get; set; } = string.Empty;
    [BsonElement("departamento")] public string Departamento { get; set; } = string.Empty;
    [BsonElement("puesto")] public string Puesto { get; set; } = string.Empty;

    [BsonElement("salario_base"), BsonRepresentation(BsonType.Decimal128)] public decimal SalarioBase { get; set; }
    [BsonElement("salario_diario"), BsonRepresentation(BsonType.Decimal128)] public decimal SalarioDiario { get; set; }

    [BsonElement("dias_laborados"), BsonRepresentation(BsonType.Decimal128)] public decimal DiasLaborados { get; set; }
    [BsonElement("dias_vacaciones"), BsonRepresentation(BsonType.Decimal128)] public decimal DiasVacaciones { get; set; }
    [BsonElement("ausencias_injustificadas")] public int AusenciasInjustificadas { get; set; }
    [BsonElement("ausencias_justificadas")] public int AusenciasJustificadas { get; set; }
    [BsonElement("horas_extra"), BsonRepresentation(BsonType.Decimal128)] public decimal HorasExtra { get; set; }

    [BsonElement("ingreso_salario"), BsonRepresentation(BsonType.Decimal128)] public decimal IngresoSalario { get; set; }
    [BsonElement("ingreso_vacaciones"), BsonRepresentation(BsonType.Decimal128)] public decimal IngresoVacaciones { get; set; }
    [BsonElement("ingreso_horas_extra"), BsonRepresentation(BsonType.Decimal128)] public decimal IngresoHorasExtra { get; set; }
    [BsonElement("total_ingresos"), BsonRepresentation(BsonType.Decimal128)] public decimal TotalIngresos { get; set; }

    [BsonElement("deduccion_ausencias"), BsonRepresentation(BsonType.Decimal128)] public decimal DeduccionAusencias { get; set; }
    [BsonElement("deduccion_permisos"), BsonRepresentation(BsonType.Decimal128)] public decimal DeduccionPermisos { get; set; }
    [BsonElement("cargas_sociales")] public List<LineaCarga> CargasSociales { get; set; } = new();
    [BsonElement("total_cargas"), BsonRepresentation(BsonType.Decimal128)] public decimal TotalCargas { get; set; }
    [BsonElement("impuesto_renta"), BsonRepresentation(BsonType.Decimal128)] public decimal ImpuestoRenta { get; set; }
    [BsonElement("total_deducciones"), BsonRepresentation(BsonType.Decimal128)] public decimal TotalDeducciones { get; set; }

    [BsonElement("salario_neto"), BsonRepresentation(BsonType.Decimal128)] public decimal SalarioNeto { get; set; }

}