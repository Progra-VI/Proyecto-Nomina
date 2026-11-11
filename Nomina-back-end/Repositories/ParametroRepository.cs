using MongoDB.Driver;
using Nomina_back_end.Entities;

namespace Nomina_back_end.Repositories;

public class ParametroRepository
{
    private readonly IMongoCollection<Parametro> _col;

    public ParametroRepository(IMongoDatabase db)
    {
        _col = db.GetCollection<Parametro>("parametros");
    }

    public async Task<List<Parametro>> ObtenerTodos() =>
        await _col.Find(_ => true).SortBy(p => p.Clave).ToListAsync();

    public async Task<Parametro?> ObtenerPorClave(string clave) =>
        await _col.Find(p => p.Clave == clave).FirstOrDefaultAsync();

    public async Task Guardar(string clave, decimal valor, string? descripcion)
    {
        var update = Builders<Parametro>.Update
            .Set(p => p.Valor, valor)
            .Set(p => p.Descripcion, descripcion);

        await _col.UpdateOneAsync(p => p.Clave == clave, update, new UpdateOptions { IsUpsert = true });
    }
}