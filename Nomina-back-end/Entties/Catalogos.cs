using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Nomina_back_end.Entities;

public abstract class CatalogoBase
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [BsonElement("descripcion")]
    public string? Descripcion { get; set; }
}

public class TipoPermiso : CatalogoBase
{
}

public class TipoHoraExtra : CatalogoBase
{
    // Multiplicador sobre el valor hora: 1.5 = tiempo y medio, 2.0 = doble
    [BsonElement("recargo")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Recargo { get; set; } = 1.5m;
}

public class MotivoLiquidacion : CatalogoBase
{
    [BsonElement("paga_preaviso")]
    public bool PagaPreaviso { get; set; }

    [BsonElement("paga_cesantia")]
    public bool PagaCesantia { get; set; }
}