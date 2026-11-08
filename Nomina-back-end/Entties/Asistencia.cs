
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Nomina_back_end.Entities;

public static class EstadosAsistencia
{
    public const string Pendiente = "Pendiente";
    public const string Completa = "Completa";
    public const string Tardanza = "Tardanza";
    public const string SalidaAnticipada = "SalidaAnticipada";
    public const string Ausencia = "Ausencia";
    public const string Ajustada = "Ajustada";
}

public class Asistencia
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("usuario_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UsuarioId { get; set; } = string.Empty;

    [BsonElement("fecha")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime Fecha { get; set; }

    [BsonElement("hora_entrada")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? HoraEntrada { get; set; }

    [BsonElement("hora_salida")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? HoraSalida { get; set; }

    [BsonElement("estado")]
    public string Estado { get; set; } = EstadosAsistencia.Pendiente;

    [BsonElement("minutos_tardanza")]
    public int MinutosTardanza { get; set; }

    [BsonElement("minutos_salida_anticipada")]
    public int MinutosSalidaAnticipada { get; set; }

    [BsonElement("observaciones")]
    public string? Observaciones { get; set; }

    [BsonElement("es_ajuste_manual")]
    public bool EsAjusteManual { get; set; } = false;

    [BsonElement("justificacion_ajuste")]
    public string? JustificacionAjuste { get; set; }

    [BsonElement("ajustado_por")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? AjustadoPor { get; set; }

    [BsonElement("fecha_ajuste")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? FechaAjuste { get; set; }

    [BsonElement("fecha_creacion")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    [BsonElement("fecha_actualizacion")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? FechaActualizacion { get; set; }
}
