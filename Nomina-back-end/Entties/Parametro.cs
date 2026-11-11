using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Nomina_back_end.Entities;

public class Parametro
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("clave")]
    public string Clave { get; set; } = string.Empty;

    [BsonElement("valor")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Valor { get; set; }

    [BsonElement("descripcion")]
    public string? Descripcion { get; set; }
}