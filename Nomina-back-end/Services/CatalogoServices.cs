using Nomina_back_end.Entities;
using Nomina_back_end.Repositories;

namespace Nomina_back_end.Services;

public abstract class CatalogoService<T> where T : CatalogoBase
{
    private readonly CatalogoRepository<T> _repo;

    protected CatalogoService(CatalogoRepository<T> repo)
    {
        _repo = repo;
    }

    public Task<List<T>> ObtenerTodos() => _repo.ObtenerTodos();

    public Task<T?> ObtenerPorId(string id) => _repo.ObtenerPorId(id);

    public async Task<(bool ok, string? error)> Crear(T entidad)
    {
        entidad.Nombre = entidad.Nombre.Trim();

        if (await _repo.ObtenerPorNombre(entidad.Nombre) != null)
            return (false, "Ya existe un registro con ese nombre.");

        await _repo.Crear(entidad);
        return (true, null);
    }

    public async Task<(bool ok, string? error)> Actualizar(string id, T entidad)
    {
        if (await _repo.ObtenerPorId(id) == null)
            return (false, "Registro no encontrado.");

        entidad.Id = id;
        entidad.Nombre = entidad.Nombre.Trim();

        var duplicado = await _repo.ObtenerPorNombre(entidad.Nombre);
        if (duplicado != null && duplicado.Id != id)
            return (false, "Ya existe un registro con ese nombre.");

        await _repo.Actualizar(entidad);
        return (true, null);
    }
}

public class TipoPermisoService : CatalogoService<TipoPermiso>
{
    public TipoPermisoService(TipoPermisoRepository repo) : base(repo) { }
}

public class TipoHoraExtraService : CatalogoService<TipoHoraExtra>
{
    public TipoHoraExtraService(TipoHoraExtraRepository repo) : base(repo) { }
}

public class MotivoLiquidacionService : CatalogoService<MotivoLiquidacion>
{
    public MotivoLiquidacionService(MotivoLiquidacionRepository repo) : base(repo) { }
}