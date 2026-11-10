using MongoDB.Bson;
using MongoDB.Driver;
using Nomina_back_end.Entities;

namespace Nomina_back_end.Repositories;

public class AsistenciaRepository
{
    private readonly IMongoCollection<Asistencia> _asistencias;

    public AsistenciaRepository(IMongoDatabase database)
    {
        _asistencias = database.GetCollection<Asistencia>("asistencias");
    }

    public async Task<Asistencia?> ObtenerPorUsuarioYFecha(string usuarioId, string fecha) =>
        await _asistencias.Find(a => a.UsuarioId == usuarioId && a.Fecha == fecha)
            .FirstOrDefaultAsync();

    public async Task<List<Asistencia>> Buscar(IEnumerable<string> usuarioIds, string? desde, string? hasta)
    {
        var fb = Builders<Asistencia>.Filter;
        var filtro = fb.In(a => a.UsuarioId, usuarioIds);

        // "yyyy-MM-dd" se ordena igual como texto que como fecha
        if (!string.IsNullOrWhiteSpace(desde)) filtro &= fb.Gte(a => a.Fecha, desde);
        if (!string.IsNullOrWhiteSpace(hasta)) filtro &= fb.Lte(a => a.Fecha, hasta);

        return await _asistencias.Find(filtro)
            .SortByDescending(a => a.Fecha)
            .ToListAsync();
    }

    public async Task Crear(Asistencia asistencia)
    {
        asistencia.Id = null;
        await _asistencias.InsertOneAsync(asistencia);
    }

    public async Task Reemplazar(Asistencia asistencia) =>
        await _asistencias.ReplaceOneAsync(a => a.Id == asistencia.Id, asistencia);
}