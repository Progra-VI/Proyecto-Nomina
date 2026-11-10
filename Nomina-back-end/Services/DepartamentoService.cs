using Nomina_back_end.Entities;
using Nomina_back_end.Repositories;

namespace Nomina_back_end.Services;

public class DepartamentoService
{
    private readonly DepartamentoRepository _repo;

    public DepartamentoService(DepartamentoRepository repo)
    {
        _repo = repo;
    }

    public Task<List<Departamento>> ObtenerTodos() => _repo.ObtenerTodos();

    public Task<Departamento?> ObtenerPorId(string id) => _repo.ObtenerPorId(id);

    public async Task<(bool ok, string? error)> Crear(Departamento departamento)
    {
        departamento.Nombre = departamento.Nombre.Trim();

        var existente = await _repo.ObtenerPorNombre(departamento.Nombre);
        if (existente != null)
            return (false, "Ya existe un departamento con ese nombre.");

        await _repo.Crear(departamento);
        return (true, null);
    }

    public async Task<(bool ok, string? error)> Actualizar(string id, Departamento departamento)
    {
        var actual = await _repo.ObtenerPorId(id);
        if (actual == null) return (false, "Departamento no encontrado.");

        departamento.Id = id;
        departamento.Nombre = departamento.Nombre.Trim();

        var duplicado = await _repo.ObtenerPorNombre(departamento.Nombre);
        if (duplicado != null && duplicado.Id != id)
            return (false, "Ya existe un departamento con ese nombre.");

        await _repo.Actualizar(departamento);
        return (true, null);
    }
}