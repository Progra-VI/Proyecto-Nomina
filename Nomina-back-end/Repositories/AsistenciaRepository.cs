
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

        CrearIndices();
    }

    private void CrearIndices()
    {
        var indiceUsuarioFecha = new CreateIndexModel<Asistencia>(
            Builders<Asistencia>.IndexKeys
                .Ascending(a => a.UsuarioId)
                .Ascending(a => a.Fecha),
            new CreateIndexOptions
            {
                Unique = true
            });

        _asistencias.Indexes.CreateOne(indiceUsuarioFecha);
    }

    public async Task<List<Asistencia>> ObtenerTodas()
    {
        return await _asistencias
            .Find(_ => true)
            .SortByDescending(a => a.Fecha)
            .ToListAsync();
    }

    public async Task<Asistencia?> ObtenerPorId(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return null;

        return await _asistencias
            .Find(a => a.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<Asistencia?> ObtenerPorUsuarioYFecha(
        string usuarioId,
        DateTime fecha)
    {
        return await _asistencias
            .Find(a =>
                a.UsuarioId == usuarioId &&
                a.Fecha == fecha)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Asistencia>> ObtenerPorUsuario(
        string usuarioId)
    {
        return await _asistencias
            .Find(a => a.UsuarioId == usuarioId)
            .SortByDescending(a => a.Fecha)
            .ToListAsync();
    }

    public async Task<List<Asistencia>> ObtenerPorPeriodo(
        string usuarioId,
        DateTime fechaInicio,
        DateTime fechaFin)
    {
        var filtro = Builders<Asistencia>.Filter.And(
            Builders<Asistencia>.Filter.Eq(a => a.UsuarioId, usuarioId),
            Builders<Asistencia>.Filter.Gte(a => a.Fecha, fechaInicio),
            Builders<Asistencia>.Filter.Lt(a => a.Fecha, fechaFin)
        );

        return await _asistencias
            .Find(filtro)
            .SortBy(a => a.Fecha)
            .ToListAsync();
    }

    public async Task Crear(Asistencia asistencia)
    {
        asistencia.Id = null;
        await _asistencias.InsertOneAsync(asistencia);
    }

    public async Task<bool> Actualizar(Asistencia asistencia)
    {
        var actualizacion = Builders<Asistencia>.Update
            .Set(a => a.HoraEntrada, asistencia.HoraEntrada)
            .Set(a => a.HoraSalida, asistencia.HoraSalida)
            .Set(a => a.Estado, asistencia.Estado)
            .Set(a => a.MinutosTardanza, asistencia.MinutosTardanza)
            .Set(a => a.MinutosSalidaAnticipada, asistencia.MinutosSalidaAnticipada)
            .Set(a => a.Observaciones, asistencia.Observaciones)
            .Set(a => a.EsAjusteManual, asistencia.EsAjusteManual)
            .Set(a => a.JustificacionAjuste, asistencia.JustificacionAjuste)
            .Set(a => a.AjustadoPor, asistencia.AjustadoPor)
            .Set(a => a.FechaAjuste, asistencia.FechaAjuste)
            .Set(a => a.FechaActualizacion, DateTime.UtcNow);

        var resultado = await _asistencias.UpdateOneAsync(
            a => a.Id == asistencia.Id,
            actualizacion
        );

        return resultado.MatchedCount > 0;
    }
}
