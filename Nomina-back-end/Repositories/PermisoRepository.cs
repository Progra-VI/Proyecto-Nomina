using MongoDB.Bson;
using MongoDB.Driver;
using Nomina_back_end.Entities;

namespace Nomina_back_end.Repositories;

public class PermisoRepository
{
    private readonly IMongoCollection<SolicitudPermiso> _col;

    public PermisoRepository(IMongoDatabase db)
    {
        _col = db.GetCollection<SolicitudPermiso>("solicitudes_permiso");
    }

    public async Task<SolicitudPermiso?> ObtenerPorId(string id)
    {
        if (!ObjectId.TryParse(id, out _)) return null;
        return await _col.Find(s => s.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<SolicitudPermiso>> Buscar(
        IEnumerable<string> usuarioIds, string? estado, string? tipoId,
        DateTime? desde, DateTime? hasta)
    {
        var fb = Builders<SolicitudPermiso>.Filter;
        var filtro = fb.In(s => s.UsuarioId, usuarioIds);

        if (!string.IsNullOrWhiteSpace(estado)) filtro &= fb.Eq(s => s.Estado, estado);
        if (!string.IsNullOrWhiteSpace(tipoId)) filtro &= fb.Eq(s => s.TipoPermisoId, tipoId);
        if (desde != null) filtro &= fb.Gte(s => s.FechaFin, desde.Value);
        if (hasta != null) filtro &= fb.Lte(s => s.FechaInicio, hasta.Value);

        return await _col.Find(filtro).SortByDescending(s => s.FechaInicio).ToListAsync();
    }

    public async Task<bool> ExisteTraslape(string usuarioId, DateTime inicio, DateTime fin, string? excluirId)
    {
        var fb = Builders<SolicitudPermiso>.Filter;
        var filtro = fb.Eq(s => s.UsuarioId, usuarioId)
            & fb.In(s => s.Estado, new[] { EstadosSolicitud.Pendiente, EstadosSolicitud.Aprobada })
            & fb.Lt(s => s.FechaInicio, fin)
            & fb.Gt(s => s.FechaFin, inicio);

        if (excluirId != null) filtro &= fb.Ne(s => s.Id, excluirId);

        return await _col.CountDocumentsAsync(filtro) > 0;
    }

    public async Task Crear(SolicitudPermiso s)
    {
        s.Id = null;
        await _col.InsertOneAsync(s);
    }

    // Solo modifica si sigue Pendiente (evita pisar una resolución simultánea)
    public async Task<bool> ActualizarSiPendiente(SolicitudPermiso s)
    {
        var filtro = Builders<SolicitudPermiso>.Filter
            .Where(x => x.Id == s.Id && x.Estado == EstadosSolicitud.Pendiente);

        var update = Builders<SolicitudPermiso>.Update
            .Set(x => x.TipoPermisoId, s.TipoPermisoId)
            .Set(x => x.FechaInicio, s.FechaInicio)
            .Set(x => x.FechaFin, s.FechaFin)
            .Set(x => x.Motivo, s.Motivo)
            .Set(x => x.ConGoce, s.ConGoce);

        var r = await _col.UpdateOneAsync(filtro, update);
        return r.MatchedCount > 0;
    }

    public async Task<bool> CambiarEstadoSiPendiente(
        string id, string estado, string? aprobadorId, string? observacion, CambioEstado cambio)
    {
        var filtro = Builders<SolicitudPermiso>.Filter
            .Where(x => x.Id == id && x.Estado == EstadosSolicitud.Pendiente);

        var update = Builders<SolicitudPermiso>.Update
            .Set(x => x.Estado, estado)
            .Set(x => x.AprobadorId, aprobadorId)
            .Set(x => x.Observacion, observacion)
            .Set(x => x.FechaResolucion, cambio.Fecha)
            .Push(x => x.Historial, cambio);

        var r = await _col.UpdateOneAsync(filtro, update);
        return r.MatchedCount > 0;
    }
}