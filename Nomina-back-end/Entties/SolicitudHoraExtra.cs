using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Nomina_back_end.Entities;

public class SolicitudHoraExtra
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("usuario_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UsuarioId { get; set; } = string.Empty;

    [BsonElement("tipo_hora_extra_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string TipoHoraExtraId { get; set; } = string.Empty;

    // "yyyy-MM-dd" en hora de Costa Rica
    [BsonElement("fecha")]
    public string Fecha { get; set; } = string.Empty;

    [BsonElement("inicio")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime Inicio { get; set; }

    [BsonElement("fin")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime Fin { get; set; }

    [BsonElement("horas")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Horas { get; set; }

    [BsonElement("justificacion")]
    public string Justificacion { get; set; } = string.Empty;

    // Estimados mientras está Pendiente; definitivos al aprobarse
    [BsonElement("valor_hora")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal ValorHora { get; set; }

    [BsonElement("recargo")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Recargo { get; set; }

    [BsonElement("monto")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Monto { get; set; }

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