using System.Globalization;
using Nomina_back_end.Entities;
using Nomina_back_end.Repositories;

namespace Nomina_back_end.Services;

public class FeriadoService
{
    private readonly FeriadoRepository _repo;

    public FeriadoService(FeriadoRepository repo)
    {
        _repo = repo;
    }

    public Task<List<Feriado>> ObtenerPorAnio(int anio) => _repo.ObtenerPorAnio(anio);

    public Task<Feriado?> ObtenerPorId(string id) => _repo.ObtenerPorId(id);

    public async Task<(bool ok, string? error)> Crear(Feriado f)
    {
        var error = await Validar(f, null);
        if (error != null) return (false, error);

        await _repo.Crear(f);
        return (true, null);
    }

    public async Task<(bool ok, string? error)> Actualizar(string id, Feriado f)
    {
        if (await _repo.ObtenerPorId(id) == null)
            return (false, "Feriado no encontrado.");

        f.Id = id;
        var error = await Validar(f, id);
        if (error != null) return (false, error);

        await _repo.Actualizar(f);
        return (true, null);
    }

    private async Task<string?> Validar(Feriado f, string? idActual)
    {
        if (!DateTime.TryParseExact(f.Fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out _))
            return "Fecha inválida.";

        f.Nombre = f.Nombre.Trim();

        var existente = await _repo.ObtenerPorFecha(f.Fecha);
        if (existente != null && existente.Id != idActual)
            return "Ya existe un feriado en esa fecha.";

        return null;
    }
}