using MongoDB.Bson;
using MongoDB.Driver;
using Nomina_back_end.Entities;

namespace Nomina_back_end.Repositories;

public class DepartamentoRepository
{
    private readonly IMongoCollection<Departamento> _departamentos;

    public DepartamentoRepository(IMongoDatabase database)
    {
        _departamentos = database.GetCollection<Departamento>("departamentos");
    }

    public async Task<List<Departamento>> ObtenerTodos() =>
        await _departamentos.Find(_ => true)
            .SortBy(d => d.Nombre)
            .ToListAsync();

    public async Task<Departamento?> ObtenerPorId(string id)
    {
        if (!ObjectId.TryParse(id, out _)) return null;
        return await _departamentos.Find(d => d.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Departamento?> ObtenerPorNombre(string nombre) =>
        await _departamentos.Find(d => d.Nombre == nombre).FirstOrDefaultAsync();

    public async Task Crear(Departamento departamento)
    {
        departamento.Id = null;
        await _departamentos.InsertOneAsync(departamento);
    }

    public async Task<bool> Actualizar(Departamento d)
    {
        var update = Builders<Departamento>.Update
            .Set(x => x.Nombre, d.Nombre)
            .Set(x => x.Descripcion, d.Descripcion);

        var r = await _departamentos.UpdateOneAsync(x => x.Id == d.Id, update);
        return r.MatchedCount > 0;
    }
}