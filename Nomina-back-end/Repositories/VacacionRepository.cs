using MongoDB.Bson;
using MongoDB.Driver;
using Nomina_back_end.Entities;

namespace Nomina_back_end.Repositories;

public class VacacionRepository
{
    private readonly IMongoCollection<SolicitudVacaciones> _col;

    public VacacionRepository(IMongoDatabase db)
    {
        _col = db.GetCollection<SolicitudVacaciones>("solicitudes_vacaciones");
    }

    public async Task<SolicitudVacaciones?> ObtenerPorId(string id)
    {
        if (!ObjectId.TryParse(id, out _)) return null;
        return await _col.Find(s => s.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<SolicitudVacaciones>> ObtenerPorUsuario(string usuarioId) =>
        await _col.Find(s => s.UsuarioId == usuarioId).SortByDescending(s => s.FechaInicio).ToListAsync();

    public async Task<List<SolicitudVacaciones>> Buscar(
        IEnumerable<string> usuarioIds, string? estado, DateTime? desde, DateTime? hasta)
    {
        var fb = Builders<SolicitudVacaciones>.Filter;
        var filtro = fb.In(s => s.UsuarioId, usuarioIds);

        if (!string.IsNullOrWhiteSpace(estado)) filtro &= fb.Eq(s => s.Estado, estado);
        if (desde != null) filtro &= fb.Gte(s => s.FechaFin, desde.Value);
        if (hasta != null) filtro &= fb.Lte(s => s.FechaInicio, hasta.Value);

        return await _col.Find(filtro).SortByDescending(s => s.FechaInicio).ToListAsync();
    }

    public async Task<bool> ExisteTraslape(string usuarioId, DateTime inicio, DateTime fin, string? excluirId)
    {
        var fb = Builders<SolicitudVacaciones>.Filter;
        var filtro = fb.Eq(s => s.UsuarioId, usuarioId)
            & fb.In(s => s.Estado, new[] { EstadosSolicitud.Pendiente, EstadosSolicitud.Aprobada })
            & fb.Lt(s => s.FechaInicio, fin)
            & fb.Gt(s => s.FechaFin, inicio);

        if (excluirId != null) filtro &= fb.Ne(s => s.Id, excluirId);

        return await _col.CountDocumentsAsync(filtro) > 0;
    }

    public async Task Crear(SolicitudVacaciones s)
    {
        s.Id = null;
        await _col.InsertOneAsync(s);
    }

    public async Task<bool> ActualizarSiPendiente(SolicitudVacaciones s)
    {
        var filtro = Builders<SolicitudVacaciones>.Filter
            .Where(x => x.Id == s.Id && x.Estado == EstadosSolicitud.Pendiente);

        var update = Builders<SolicitudVacaciones>.Update
            .Set(x => x.FechaInicio, s.FechaInicio)
            .Set(x => x.FechaFin, s.FechaFin)
            .Set(x => x.Dias, s.Dias)
            .Set(x => x.Motivo, s.Motivo);

        var r = await _col.UpdateOneAsync(filtro, update);
        return r.MatchedCount > 0;
    }

    public async Task<bool> CambiarEstadoSiPendiente(
        string id, string estado, string? aprobadorId, string? observacion, CambioEstado cambio)
    {
        var filtro = Builders<SolicitudVacaciones>.Filter
            .Where(x => x.Id == id && x.Estado == EstadosSolicitud.Pendiente);

        var update = Builders<SolicitudVacaciones>.Update
            .Set(x => x.Estado, estado)
            .Set(x => x.AprobadorId, aprobadorId)
            .Set(x => x.Observacion, observacion)
            .Set(x => x.FechaResolucion, cambio.Fecha)
            .Push(x => x.Historial, cambio);

        var r = await _col.UpdateOneAsync(filtro, update);
        return r.MatchedCount > 0;
    }
}