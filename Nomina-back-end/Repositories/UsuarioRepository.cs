using MongoDB.Bson;
using MongoDB.Driver;
using Nomina_back_end.Entities;

namespace Nomina_back_end.Repositories;

public class UsuarioRepository
{
    private readonly IMongoCollection<Usuario> _usuarios;

    public UsuarioRepository(IMongoDatabase database)
    {
        _usuarios = database.GetCollection<Usuario>("usuarios");
        CrearIndices();
    }

    private void CrearIndices()
    {
        var indices = new[]
        {
            new CreateIndexModel<Usuario>(
                Builders<Usuario>.IndexKeys.Ascending(u => u.Correo),
                new CreateIndexOptions { Unique = true }),
            new CreateIndexModel<Usuario>(
                Builders<Usuario>.IndexKeys.Ascending(u => u.Identificacion),
                new CreateIndexOptions { Unique = true })
        };
        _usuarios.Indexes.CreateMany(indices);
    }

    public async Task<List<Usuario>> ObtenerTodos() =>
        await _usuarios.Find(_ => true).ToListAsync();

    public async Task<Usuario?> ObtenerPorId(string id)
    {
        if (!ObjectId.TryParse(id, out _)) return null;
        return await _usuarios.Find(u => u.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Usuario?> ObtenerPorCorreo(string correo) =>
        await _usuarios.Find(u => u.Correo == correo).FirstOrDefaultAsync();

    public async Task<Usuario?> ObtenerPorIdentificacion(string identificacion) =>
        await _usuarios.Find(u => u.Identificacion == identificacion).FirstOrDefaultAsync();

    public async Task<List<Usuario>> ObtenerPorJefatura(string jefaturaId) =>
        await _usuarios.Find(u => u.JefaturaDirectaId == jefaturaId).ToListAsync();

    public async Task Crear(Usuario usuario)
    {
        usuario.Id = null;
        await _usuarios.InsertOneAsync(usuario);
    }

    public async Task<bool> Actualizar(Usuario u)
    {
        var update = Builders<Usuario>.Update
            .Set(x => x.Identificacion, u.Identificacion)
            .Set(x => x.Nombre, u.Nombre)
            .Set(x => x.Correo, u.Correo)
            .Set(x => x.Rol, u.Rol)
            .Set(x => x.DepartamentoId, u.DepartamentoId)
            .Set(x => x.Puesto, u.Puesto)
            .Set(x => x.JefaturaDirectaId, u.JefaturaDirectaId)
            .Set(x => x.FechaIngreso, u.FechaIngreso)
            .Set(x => x.SalarioBase, u.SalarioBase)
            .Set(x => x.TipoJornada, u.TipoJornada)
            .Set(x => x.Horario, u.Horario)
            .Set(x => x.FotoUrl, u.FotoUrl)
            .Set(x => x.FechaActualizacion, DateTime.UtcNow);

        var r = await _usuarios.UpdateOneAsync(x => x.Id == u.Id, update);
        return r.MatchedCount > 0;
    }

    public async Task<bool> CambiarEstado(string id, string estado)
    {
        var update = Builders<Usuario>.Update
            .Set(x => x.Estado, estado)
            .Set(x => x.FechaActualizacion, DateTime.UtcNow);

        var r = await _usuarios.UpdateOneAsync(x => x.Id == id, update);
        return r.MatchedCount > 0;
    }
}