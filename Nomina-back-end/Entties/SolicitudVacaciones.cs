using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Nomina_back_end.Entities;

public class SolicitudVacaciones
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("usuario_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UsuarioId { get; set; } = string.Empty;

    // Inicio: 00:00 del primer día. Fin: 00:00 del día siguiente al último (hora CR, guardado en UTC)
    [BsonElement("fecha_inicio")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime FechaInicio { get; set; }

    [BsonElement("fecha_fin")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime FechaFin { get; set; }

    [BsonElement("dias")]
    public int Dias { get; set; }

    [BsonElement("motivo")]
    public string? Motivo { get; set; }

    [BsonElement("estado")]
    public string Estado { get; set; } = EstadosSolicitud.Pendiente;

    [BsonElement("aprobador_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? AprobadorId { get; set; }

    [BsonElement("observacion")]
    public string? Observacion { get; set; }

    [BsonElement("fecha_resolucion")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? FechaResolucion { get; set; }

    [BsonElement("fecha_creacion")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    [BsonElement("historial")]
    public List<CambioEstado> Historial { get; set; } = new();
}