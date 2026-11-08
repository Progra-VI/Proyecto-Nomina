
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

    public Task<List<Departamento>> ObtenerTodos()
    {
        return _repo.ObtenerTodos();
    }

    public async Task<Departamento?> ObtenerPorId(string id)
    {
        return await _repo.ObtenerPorId(id);
    }

    public async Task<(bool ok, string? error)> Crear(Departamento departamento)
    {
        departamento.Nombre = departamento.Nombre.Trim();
        departamento.Descripcion = departamento.Descripcion.Trim();

        var error = await ValidarNombre(departamento.Nombre, null);

        if (error != null)
            return (false, error);

        departamento.Estado = EstadosDepartamento.Activo;
        departamento.FechaCreacion = DateTime.UtcNow;

        await _repo.Crear(departamento);

        return (true, null);
    }

    public async Task<(bool ok, string? error)> Actualizar(
        string id,
        Departamento departamento)
    {
        var existente = await _repo.ObtenerPorId(id);

        if (existente == null)
            return (false, "Departamento no encontrado.");

        departamento.Nombre = departamento.Nombre.Trim();
        departamento.Descripcion = departamento.Descripcion.Trim();
        departamento.Id = id;

        var error = await ValidarNombre(departamento.Nombre, id);

        if (error != null)
            return (false, error);

        var actualizado = await _repo.Actualizar(departamento);

        if (!actualizado)
            return (false, "No fue posible actualizar el departamento.");

        return (true, null);
    }

    public async Task<(bool ok, string? error)> Inactivar(string id)
    {
        var existente = await _repo.ObtenerPorId(id);

        if (existente == null)
            return (false, "Departamento no encontrado.");

        if (existente.Estado == EstadosDepartamento.Inactivo)
            return (false, "El departamento ya está inactivo.");

        var actualizado = await _repo.CambiarEstado(
            id,
            EstadosDepartamento.Inactivo);

        if (!actualizado)
            return (false, "No fue posible inactivar el departamento.");

        return (true, null);
    }

    private async Task<string?> ValidarNombre(
        string nombre,
        string? idActual)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return "El nombre del departamento es obligatorio.";

        var existente = await _repo.ObtenerPorNombre(nombre);

        if (existente != null && existente.Id != idActual)
            return "Ya existe un departamento con ese nombre.";

        return null;
    }
}
