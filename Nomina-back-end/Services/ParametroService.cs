using Nomina_back_end.Entities;
using Nomina_back_end.Repositories;

namespace Nomina_back_end.Services;

public class ParametroService
{
    private readonly ParametroRepository _repo;

    public ParametroService(ParametroRepository repo)
    {
        _repo = repo;
    }

    public Task<List<Parametro>> ObtenerTodos() => _repo.ObtenerTodos();

    public async Task<decimal?> ObtenerValor(string clave) =>
        (await _repo.ObtenerPorClave(clave.ToLower()))?.Valor;

    public async Task Guardar(string clave, decimal valor, string? descripcion) =>
        await _repo.Guardar(clave.ToLower(), valor, descripcion?.Trim());
}