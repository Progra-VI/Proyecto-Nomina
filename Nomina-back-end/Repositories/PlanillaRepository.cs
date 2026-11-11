using MongoDB.Bson;
using MongoDB.Driver;
using Nomina_back_end.Entities;

namespace Nomina_back_end.Repositories;

public class PlanillaRepository
{
    private readonly IMongoCollection<Planilla> _col;

    public PlanillaRepository(IMongoDatabase db)
    {
        _col = db.GetCollection<Planilla>("planillas");
    }

    public async Task<Planilla?> ObtenerPorId(string id)
    {
        if (!ObjectId.TryParse(id, out _)) return null;
        return await _col.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Planilla?> ObtenerPorPeriodo(int anio, int mes, int? quincena)
    {
        var fb = Builders<Planilla>.Filter;
        var filtro = fb.Eq(p => p.Anio, anio) & fb.Eq(p => p.Mes, mes) & fb.Eq(p => p.Quincena, quincena);
        return await _col.Find(filtro).FirstOrDefaultAsync();
    }

    public async Task<List<Planilla>> Listar(int? anio)
    {
        var filtro = anio == null
            ? Builders<Planilla>.Filter.Empty
            : Builders<Planilla>.Filter.Eq(p => p.Anio, anio.Value);

        return await _col.Find(filtro).SortByDescending(p => p.Desde).ToListAsync();
    }

    public async Task Crear(Planilla p)
    {
        p.Id = null;
        await _col.InsertOneAsync(p);
    }

    // Solo guarda si la planilla sigue siendo editable (Borrador o Calculada)
    public async Task<bool> GuardarCalculo(Planilla p, CambioEstado cambio)
    {
        var filtro = Builders<Planilla>.Filter.Where(x => x.Id == p.Id &&
            (x.Estado == EstadosPlanilla.Borrador || x.Estado == EstadosPlanilla.Calculada));

        var update = Builders<Planilla>.Update
            .Set(x => x.Estado, EstadosPlanilla.Calculada)
            .Set(x => x.Empleados, p.Empleados)
            .Set(x => x.TotalIngresos, p.TotalIngresos)
            .Set(x => x.TotalDeducciones, p.TotalDeducciones)
            .Set(x => x.TotalNeto, p.TotalNeto)
            .Set(x => x.FechaCalculo, p.FechaCalculo)
            .Push(x => x.Historial, cambio);

        var r = await _col.UpdateOneAsync(filtro, update);
        return r.MatchedCount > 0;
    }

    public async Task<bool> CambiarEstado(string id, string desde, string hasta, CambioEstado cambio)
    {
        var filtro = Builders<Planilla>.Filter.Where(x => x.Id == id && x.Estado == desde);

        var update = Builders<Planilla>.Update
            .Set(x => x.Estado, hasta)
            .Push(x => x.Historial, cambio);

        var r = await _col.UpdateOneAsync(filtro, update);
        return r.MatchedCount > 0;
    }
}