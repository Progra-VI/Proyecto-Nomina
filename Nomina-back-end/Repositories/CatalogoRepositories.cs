using MongoDB.Bson;
using MongoDB.Driver;
using Nomina_back_end.Entities;

namespace Nomina_back_end.Repositories;

public abstract class CatalogoRepository<T> where T : CatalogoBase
{
    private readonly IMongoCollection<T> _coleccion;

    protected CatalogoRepository(IMongoDatabase database, string nombreColeccion)
    {
        _coleccion = database.GetCollection<T>(nombreColeccion);
    }

    public async Task<List<T>> ObtenerTodos() =>
        await _coleccion.Find(_ => true).SortBy(x => x.Nombre).ToListAsync();

    public async Task<T?> ObtenerPorId(string id)
    {
        if (!ObjectId.TryParse(id, out _)) return null;
        return await _coleccion.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<T?> ObtenerPorNombre(string nombre) =>
        await _coleccion.Find(x => x.Nombre == nombre).FirstOrDefaultAsync();

    public async Task Crear(T entidad)
    {
        entidad.Id = null;
        await _coleccion.InsertOneAsync(entidad);
    }

    public async Task<bool> Actualizar(T entidad)
    {
        var r = await _coleccion.ReplaceOneAsync(x => x.Id == entidad.Id, entidad);
        return r.MatchedCount > 0;
    }
}

public class TipoPermisoRepository : CatalogoRepository<TipoPermiso>
{
    public TipoPermisoRepository(IMongoDatabase db) : base(db, "tipos_permiso") { }
}

public class TipoHoraExtraRepository : CatalogoRepository<TipoHoraExtra>
{
    public TipoHoraExtraRepository(IMongoDatabase db) : base(db, "tipos_hora_extra") { }
}

public class MotivoLiquidacionRepository : CatalogoRepository<MotivoLiquidacion>
{
    public MotivoLiquidacionRepository(IMongoDatabase db) : base(db, "motivos_liquidacion") { }
}