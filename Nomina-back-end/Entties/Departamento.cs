
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Nomina_back_end.Entities;

public static class EstadosDepartamento
{
    public const string Activo = "Activo";
    public const string Inactivo = "Inactivo";
}

public class Departamento
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [BsonElement("descripcion")]
    public string Descripcion { get; set; } = string.Empty;

    [BsonElement("estado")]
    public string Estado { get; set; } = EstadosDepartamento.Activo;

    [BsonElement("fecha_creacion")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    [BsonElement("fecha_actualizacion")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? FechaActualizacion { get; set; }
}
