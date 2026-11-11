using MongoDB.Bson;
using MongoDB.Driver;
using Nomina_back_end.Entities;

namespace Nomina_back_end.Repositories;

public class HoraExtraRepository
{
    private readonly IMongoCollection<SolicitudHoraExtra> _col;

    public HoraExtraRepository(IMongoDatabase db)
    {
        _col = db.GetCollection<SolicitudHoraExtra>("solicitudes_hora_extra");
    }

    public async Task<SolicitudHoraExtra?> ObtenerPorId(string id)
    {
        if (!ObjectId.TryParse(id, out _)) return null;
        return await _col.Find(s => s.Id == id).FirstOrDefaultAsync();
    }

    // desde y hasta en "yyyy-MM-dd" (la planilla usará este método con estado Aprobada)
    public async Task<List<SolicitudHoraExtra>> Buscar(
        IEnumerable<string> usuarioIds, string? estado, string? tipoId, string? desde, string? hasta)
    {
        var fb = Builders<SolicitudHoraExtra>.Filter;
        var filtro = fb.In(s => s.UsuarioId, usuarioIds);

        if (!string.IsNullOrWhiteSpace(estado)) filtro &= fb.Eq(s => s.Estado, estado);
        if (!string.IsNullOrWhiteSpace(tipoId)) filtro &= fb.Eq(s => s.TipoHoraExtraId, tipoId);
        if (!string.IsNullOrWhiteSpace(desde)) filtro &= fb.Gte(s => s.Fecha, desde);
        if (!string.IsNullOrWhiteSpace(hasta)) filtro &= fb.Lte(s => s.Fecha, hasta);

        return await _col.Find(filtro).SortByDescending(s => s.Fecha).ToListAsync();
    }

    public async Task<bool> ExisteTraslape(string usuarioId, DateTime inicio, DateTime fin, string? excluirId)
    {
        var fb = Builders<SolicitudHoraExtra>.Filter;
        var filtro = fb.Eq(s => s.UsuarioId, usuarioId)
            & fb.In(s => s.Estado, new[] { EstadosSolicitud.Pendiente, EstadosSolicitud.Aprobada })
            & fb.Lt(s => s.Inicio, fin)
            & fb.Gt(s => s.Fin, inicio);

        if (excluirId != null) filtro &= fb.Ne(s => s.Id, excluirId);

        return await _col.CountDocumentsAsync(filtro) > 0;
    }

    public async Task Crear(SolicitudHoraExtra s)
    {
        s.Id = null;
        await _col.InsertOneAsync(s);
    }

    public async Task<bool> ActualizarSiPendiente(SolicitudHoraExtra s)
    {
        var filtro = Builders<SolicitudHoraExtra>.Filter
            .Where(x => x.Id == s.Id && x.Estado == EstadosSolicitud.Pendiente);

        var update = Builders<SolicitudHoraExtra>.Update
            .Set(x => x.TipoHoraExtraId, s.TipoHoraExtraId)
            .Set(x => x.Fecha, s.Fecha)
            .Set(x => x.Inicio, s.Inicio)
            .Set(x => x.Fin, s.Fin)
            .Set(x => x.Horas, s.Horas)
            .Set(x => x.Justificacion, s.Justificacion)
            .Set(x => x.ValorHora, s.ValorHora)
            .Set(x => x.Recargo, s.Recargo)
            .Set(x => x.Monto, s.Monto);

        var r = await _col.UpdateOneAsync(filtro, update);
        return r.MatchedCount > 0;
    }

    // Si llegan valorHora, recargo y monto (aprobación), quedan guardados como definitivos
    public async Task<bool> CambiarEstadoSiPendiente(
        string id, string estado, string? aprobadorId, string? observacion, CambioEstado cambio,
        decimal? valorHora = null, decimal? recargo = null, decimal? monto = null)
    {
        var filtro = Builders<SolicitudHoraExtra>.Filter
            .Where(x => x.Id == id && x.Estado == EstadosSolicitud.Pendiente);

        var ub = Builders<SolicitudHoraExtra>.Update;
        var cambios = new List<UpdateDefinition<SolicitudHoraExtra>>
        {
            ub.Set(x => x.Estado, estado),
            ub.Set(x => x.AprobadorId, aprobadorId),
            ub.Set(x => x.Observacion, observacion),
            ub.Set(x => x.FechaResolucion, cambio.Fecha),
            ub.Push(x => x.Historial, cambio)
        };

        if (valorHora != null && recargo != null && monto != null)
        {
            cambios.Add(ub.Set(x => x.ValorHora, valorHora.Value));
            cambios.Add(ub.Set(x => x.Recargo, recargo.Value));
            cambios.Add(ub.Set(x => x.Monto, monto.Value));
        }

        var r = await _col.UpdateOneAsync(filtro, ub.Combine(cambios));
        return r.MatchedCount > 0;
    }
}