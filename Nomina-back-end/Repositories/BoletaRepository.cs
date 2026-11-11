using MongoDB.Bson;
using MongoDB.Driver;
using Nomina_back_end.Entities;

namespace Nomina_back_end.Repositories;

public class BoletaRepository
{
    private readonly IMongoCollection<BoletaPago> _col;

    public BoletaRepository(IMongoDatabase db)
    {
        _col = db.GetCollection<BoletaPago>("boletas_pago");
    }

    public async Task<BoletaPago?> ObtenerPorId(string id)
    {
        if (!ObjectId.TryParse(id, out _)) return null;
        return await _col.Find(b => b.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<BoletaPago>> ObtenerPorPlanilla(string planillaId) =>
        await _col.Find(b => b.PlanillaId == planillaId).SortBy(b => b.NombreEmpleado).ToListAsync();

    public async Task<List<BoletaPago>> ObtenerPorUsuario(string usuarioId) =>
        await _col.Find(b => b.UsuarioId == usuarioId).ToListAsync();

    public async Task ReemplazarDePlanilla(string planillaId, List<BoletaPago> boletas)
    {
        await _col.DeleteManyAsync(b => b.PlanillaId == planillaId);

        foreach (var b in boletas) b.Id = null;
        if (boletas.Count > 0) await _col.InsertManyAsync(boletas);
    }
}