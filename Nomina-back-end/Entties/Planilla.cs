using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Nomina_back_end.Entities;

public static class EstadosPlanilla
{
    public const string Borrador = "Borrador";
    public const string Calculada = "Calculada";
    public const string Aprobada = "Aprobada";
    public const string Pagada = "Pagada";
}

public class Planilla
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("anio")]
    public int Anio { get; set; }

    [BsonElement("mes")]
    public int Mes { get; set; }

    // null si la planilla es mensual
    [BsonElement("quincena")]
    public int? Quincena { get; set; }

    [BsonElement("desde")]
    public string Desde { get; set; } = string.Empty;

    [BsonElement("hasta")]
    public string Hasta { get; set; } = string.Empty;

    [BsonElement("estado")]
    public string Estado { get; set; } = EstadosPlanilla.Borrador;

    [BsonElement("empleados")]
    public int Empleados { get; set; }

    [BsonElement("total_ingresos")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal TotalIngresos { get; set; }

    [BsonElement("total_deducciones")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal TotalDeducciones { get; set; }

    [BsonElement("total_neto")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal TotalNeto { get; set; }

    [BsonElement("fecha_calculo")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? FechaCalculo { get; set; }

    [BsonElement("fecha_creacion")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    [BsonElement("historial")]
    public List<CambioEstado> Historial { get; set; } = new();
}