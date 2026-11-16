using MongoDB.Bson;
using MongoDB.Driver;
using Nomina_back_end.Entities;

namespace Nomina_back_end.Repositories;

public class LiquidacionRepository
{
    private readonly IMongoCollection<Liquidacion> _col;

    public LiquidacionRepository(IMongoDatabase db)
    {
        _col = db.GetCollection<Liquidacion>("liquidaciones");
    }

    public async Task<Liquidacion?> ObtenerPorId(string id)
    {
        if (!ObjectId.TryParse(id, out _)) return null;
        return await _col.Find(l => l.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<Liquidacion>> Listar(string? estado, string? usuarioId)
    {
        var fb = Builders<Liquidacion>.Filter;
        var filtro = fb.Empty;

        if (!string.IsNullOrWhiteSpace(estado)) filtro &= fb.Eq(l => l.Estado, estado);
        if (!string.IsNullOrWhiteSpace(usuarioId)) filtro &= fb.Eq(l => l.UsuarioId, usuarioId);

        return await _col.Find(filtro).SortByDescending(l => l.FechaCreacion).ToListAsync();
    }

    public async Task<bool> ExistePorUsuario(string usuarioId) =>
        await _col.CountDocumentsAsync(l => l.UsuarioId == usuarioId) > 0;

    public async Task Crear(Liquidacion l)
    {
        l.Id = null;
        await _col.InsertOneAsync(l);
    }

    public async Task<bool> ReemplazarSiBorrador(Liquidacion l)
    {
        var filtro = Builders<Liquidacion>.Filter
            .Where(x => x.Id == l.Id && x.Estado == EstadosLiquidacion.Borrador);

        var r = await _col.ReplaceOneAsync(filtro, l);
        return r.MatchedCount > 0;
    }

    public async Task<bool> EliminarSiBorrador(string id)
    {
        var r = await _col.DeleteOneAsync(x => x.Id == id && x.Estado == EstadosLiquidacion.Borrador);
        return r.DeletedCount > 0;
    }

    public async Task<bool> CambiarEstado(
        string id, string desde, string hasta, CambioEstado cambio, string? comprobanteId = null)
    {
        var filtro = Builders<Liquidacion>.Filter.Where(x => x.Id == id && x.Estado == desde);

        var ub = Builders<Liquidacion>.Update;
        var cambios = new List<UpdateDefinition<Liquidacion>>
        {
            ub.Set(x => x.Estado, hasta),
            ub.Push(x => x.Historial, cambio)
        };

        if (comprobanteId != null) cambios.Add(ub.Set(x => x.ComprobanteId, comprobanteId));

        var r = await _col.UpdateOneAsync(filtro, ub.Combine(cambios));
        return r.MatchedCount > 0;
    }
}