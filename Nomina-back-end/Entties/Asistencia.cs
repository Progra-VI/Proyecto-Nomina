using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Nomina_back_end.Entities;

public class AjusteAsistencia
{
    [BsonElement("ajustado_por_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string AjustadoPorId { get; set; } = string.Empty;

    [BsonElement("fecha_ajuste")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime FechaAjuste { get; set; } = DateTime.UtcNow;

    [BsonElement("justificacion")]
    public string Justificacion { get; set; } = string.Empty;

    [BsonElement("entrada_anterior")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? EntradaAnterior { get; set; }

    [BsonElement("salida_anterior")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? SalidaAnterior { get; set; }
}

public class Asistencia
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("usuario_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UsuarioId { get; set; } = string.Empty;

    // "yyyy-MM-dd" en hora de Costa Rica (evita problemas de zona horaria)
    [BsonElement("fecha")]
    public string Fecha { get; set; } = string.Empty;

    [BsonElement("hora_entrada")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? HoraEntrada { get; set; }

    [BsonElement("hora_salida")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? HoraSalida { get; set; }

    [BsonElement("tardia")]
    public bool Tardia { get; set; }

    [BsonElement("minutos_tardia")]
    public int MinutosTardia { get; set; }

    [BsonElement("salida_anticipada")]
    public bool SalidaAnticipada { get; set; }

    [BsonElement("minutos_salida_anticipada")]
    public int MinutosSalidaAnticipada { get; set; }

    [BsonElement("horas_trabajadas")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal HorasTrabajadas { get; set; }

    [BsonElement("ajustes")]
    public List<AjusteAsistencia> Ajustes { get; set; } = new();
}