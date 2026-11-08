
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

        CrearIndices();
    }

    private void CrearIndices()
    {
        var indiceNombre = new CreateIndexModel<Departamento>(
            Builders<Departamento>.IndexKeys
                .Ascending(d => d.Nombre),
            new CreateIndexOptions
            {
                Unique = true
            });

        _departamentos.Indexes.CreateOne(indiceNombre);
    }

    public async Task<List<Departamento>> ObtenerTodos()
    {
        return await _departamentos
            .Find(_ => true)
            .SortBy(d => d.Nombre)
            .ToListAsync();
    }

    public async Task<Departamento?> ObtenerPorId(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return null;

        return await _departamentos
            .Find(d => d.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<Departamento?> ObtenerPorNombre(string nombre)
    {
        return await _departamentos
            .Find(d => d.Nombre == nombre)
            .FirstOrDefaultAsync();
    }

    public async Task Crear(Departamento departamento)
    {
        departamento.Id = null;
        await _departamentos.InsertOneAsync(departamento);
    }

    public async Task<bool> Actualizar(Departamento departamento)
    {
        var actualizacion = Builders<Departamento>.Update
            .Set(d => d.Nombre, departamento.Nombre)
            .Set(d => d.Descripcion, departamento.Descripcion)
            .Set(d => d.FechaActualizacion, DateTime.UtcNow);

        var resultado = await _departamentos.UpdateOneAsync(
            d => d.Id == departamento.Id,
            actualizacion
        );

        return resultado.MatchedCount > 0;
    }

    public async Task<bool> CambiarEstado(string id, string estado)
    {
        var actualizacion = Builders<Departamento>.Update
            .Set(d => d.Estado, estado)
            .Set(d => d.FechaActualizacion, DateTime.UtcNow);

        var resultado = await _departamentos.UpdateOneAsync(
            d => d.Id == id,
            actualizacion
        );

        return resultado.MatchedCount > 0;
    }
}
