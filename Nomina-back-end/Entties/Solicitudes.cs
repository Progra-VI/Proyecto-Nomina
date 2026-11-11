using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Nomina_back_end.Entities;

public static class EstadosSolicitud
{
    public const string Pendiente = "Pendiente";
    public const string Aprobada = "Aprobada";
    public const string Rechazada = "Rechazada";
    public const string Cancelada = "Cancelada";
}

public class CambioEstado
{
    [BsonElement("estado")]
    public string Estado { get; set; } = string.Empty;

    [BsonElement("usuario_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UsuarioId { get; set; } = string.Empty;

    [BsonElement("fecha")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    [BsonElement("observacion")]
    public string? Observacion { get; set; }
}

public class SolicitudPermiso
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("usuario_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UsuarioId { get; set; } = string.Empty;

    [BsonElement("tipo_permiso_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string TipoPermisoId { get; set; } = string.Empty;

    [BsonElement("fecha_inicio")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime FechaInicio { get; set; }

    [BsonElement("fecha_fin")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime FechaFin { get; set; }

    [BsonElement("motivo")]
    public string Motivo { get; set; } = string.Empty;

    [BsonElement("con_goce")]
    public bool ConGoce { get; set; }

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