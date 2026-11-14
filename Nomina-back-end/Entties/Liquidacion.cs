using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Nomina_back_end.Entities;

public static class EstadosLiquidacion
{
    public const string Borrador = "Borrador";
    public const string Aprobada = "Aprobada";
    public const string Pagada = "Pagada";
}

public class RubroLiquidacion
{
    [BsonElement("concepto")]
    public string Concepto { get; set; } = string.Empty;

    [BsonElement("detalle")]
    public string Detalle { get; set; } = string.Empty;

    [BsonElement("monto")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Monto { get; set; }
}

public class Liquidacion
{
    [BsonId, BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("usuario_id"), BsonRepresentation(BsonType.ObjectId)]
    public string UsuarioId { get; set; } = string.Empty;

    [BsonElement("motivo_id"), BsonRepresentation(BsonType.ObjectId)]
    public string MotivoId { get; set; } = string.Empty;

    [BsonElement("motivo")] public string Motivo { get; set; } = string.Empty;

    // Copia de los datos del empleado al momento de calcular
    [BsonElement("nombre_empleado")] public string NombreEmpleado { get; set; } = string.Empty;
    [BsonElement("identificacion")] public string Identificacion { get; set; } = string.Empty;
    [BsonElement("departamento")] public string Departamento { get; set; } = string.Empty;
    [BsonElement("puesto")] public string Puesto { get; set; } = string.Empty;
    [BsonElement("fecha_ingreso")] public string FechaIngreso { get; set; } = string.Empty;
    [BsonElement("fecha_salida")] public string FechaSalida { get; set; } = string.Empty;
    [BsonElement("antiguedad_meses")] public int AntiguedadMeses { get; set; }

    [BsonElement("salario_base"), BsonRepresentation(BsonType.Decimal128)]
    public decimal SalarioBase { get; set; }

    [BsonElement("ingresos")] public List<RubroLiquidacion> Ingresos { get; set; } = new();
    [BsonElement("deducciones")] public List<RubroLiquidacion> Deducciones { get; set; } = new();

    [BsonElement("total_ingresos"), BsonRepresentation(BsonType.Decimal128)]
    public decimal TotalIngresos { get; set; }

    [BsonElement("total_deducciones"), BsonRepresentation(BsonType.Decimal128)]
    public decimal TotalDeducciones { get; set; }

    [BsonElement("total_neto"), BsonRepresentation(BsonType.Decimal128)]
    public decimal TotalNeto { get; set; }

    [BsonElement("estado")]
    public string Estado { get; set; } = EstadosLiquidacion.Borrador;

    // Archivo del comprobante en GridFS (se genera al aprobar)
    [BsonElement("comprobante_id"), BsonRepresentation(BsonType.ObjectId)]
    public string? ComprobanteId { get; set; }

    [BsonElement("fecha_creacion"), BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    [BsonElement("historial")]
    public List<CambioEstado> Historial { get; set; } = new();
}