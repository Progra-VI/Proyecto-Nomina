using MongoDB.Bson;
using MongoDB.Driver;
using Nomina_back_end.Entities;

namespace Nomina_back_end.Repositories;

public class FeriadoRepository
{
    private readonly IMongoCollection<Feriado> _col;

    public FeriadoRepository(IMongoDatabase db)
    {
        _col = db.GetCollection<Feriado>("feriados");
    }

    public async Task<List<Feriado>> ObtenerPorAnio(int anio)
    {
        var fb = Builders<Feriado>.Filter;
        var filtro = fb.Gte(f => f.Fecha, $"{anio}-01-01") & fb.Lte(f => f.Fecha, $"{anio}-12-31");
        return await _col.Find(filtro).SortBy(f => f.Fecha).ToListAsync();
    }

    public async Task<Feriado?> ObtenerPorId(string id)
    {
        if (!ObjectId.TryParse(id, out _)) return null;
        return await _col.Find(f => f.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Feriado?> ObtenerPorFecha(string fecha) =>
        await _col.Find(f => f.Fecha == fecha).FirstOrDefaultAsync();

    public async Task Crear(Feriado feriado)
    {
        feriado.Id = null;
        await _col.InsertOneAsync(feriado);
    }

    public async Task<bool> Actualizar(Feriado f)
    {
        var update = Builders<Feriado>.Update
            .Set(x => x.Fecha, f.Fecha)
            .Set(x => x.Nombre, f.Nombre);
        var r = await _col.UpdateOneAsync(x => x.Id == f.Id, update);
        return r.MatchedCount > 0;
    }
}