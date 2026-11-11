using MongoDB.Driver;
using Nomina_back_end.Entities;

namespace Nomina_back_end.Repositories;

public class ConfiguracionLeyRepository
{
    private readonly IMongoCollection<ConfiguracionLey> _col;

    public ConfiguracionLeyRepository(IMongoDatabase db)
    {
        _col = db.GetCollection<ConfiguracionLey>("configuracion_ley");
    }

    public async Task<ConfiguracionLey?> Obtener() =>
        await _col.Find(c => c.Id == "ley").FirstOrDefaultAsync();

    public async Task Guardar(ConfiguracionLey config)
    {
        config.Id = "ley";
        await _col.ReplaceOneAsync(c => c.Id == "ley", config, new ReplaceOptions { IsUpsert = true });
    }
}