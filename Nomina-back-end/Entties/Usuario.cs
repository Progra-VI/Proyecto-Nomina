using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Nomina_back_end.Entities;

public static class Roles
{
    public const string Empleado = "Empleado";
    public const string Jefatura = "Jefatura";
    public const string RRHH = "RRHH";
}

public static class EstadosUsuario
{
    public const string Activo = "Activo";
    public const string Inactivo = "Inactivo";
}

public class Horario
{
    [BsonElement("hora_entrada")]
    public string HoraEntrada { get; set; } = "08:00";

    [BsonElement("hora_salida")]
    public string HoraSalida { get; set; } = "17:00";

    [BsonElement("dias_laborales")]
    public List<int> DiasLaborales { get; set; } = new() { 1, 2, 3, 4, 5 }; // 1 = lunes ... 7 = domingo
}

public class Usuario
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("identificacion")]
    public string Identificacion { get; set; } = string.Empty;

    [BsonElement("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [BsonElement("correo")]
    public string Correo { get; set; } = string.Empty;

    [BsonElement("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [BsonElement("rol")]
    public string Rol { get; set; } = Roles.Empleado;

    [BsonElement("departamento_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string DepartamentoId { get; set; } = string.Empty;

    [BsonElement("puesto")]
    public string Puesto { get; set; } = string.Empty;

    [BsonElement("jefatura_directa_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? JefaturaDirectaId { get; set; } // null si no tiene jefe

    [BsonElement("fecha_ingreso")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime FechaIngreso { get; set; }

    [BsonElement("salario_base")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal SalarioBase { get; set; }

    [BsonElement("tipo_jornada")]
    public string TipoJornada { get; set; } = string.Empty; // ej: Diurna, Mixta, Nocturna

    [BsonElement("horario")]
    public Horario Horario { get; set; } = new();

    [BsonElement("foto_url")]
    public string? FotoUrl { get; set; }

    [BsonElement("estado")]
    public string Estado { get; set; } = EstadosUsuario.Activo;

    [BsonElement("fecha_creacion")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    [BsonElement("fecha_actualizacion")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? FechaActualizacion { get; set; }
}