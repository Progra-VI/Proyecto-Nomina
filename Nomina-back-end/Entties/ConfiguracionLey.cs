using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Nomina_back_end.Entities;

public class CargaSocial
{
    [BsonElement("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [BsonElement("porcentaje")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Porcentaje { get; set; }
}

public class TramoRenta
{
    [BsonElement("desde")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Desde { get; set; }

    // null = sin límite superior
    [BsonElement("hasta")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal? Hasta { get; set; }

    [BsonElement("porcentaje")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Porcentaje { get; set; }
}

public class ConfiguracionLey
{
    [BsonId]
    public string Id { get; set; } = "ley";

    [BsonElement("cargas_sociales")]
    public List<CargaSocial> CargasSociales { get; set; } = new();

    [BsonElement("tramos_renta")]
    public List<TramoRenta> TramosRenta { get; set; } = new();

    [BsonElement("fecha_actualizacion")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

    [BsonElement("actualizado_por_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ActualizadoPorId { get; set; }
}