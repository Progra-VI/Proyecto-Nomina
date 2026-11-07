using Nomina_back_end.Entities;
using Nomina_back_end.Repositories;

namespace Nomina_back_end.Services;

public class UsuarioService
{
    private readonly UsuarioRepository _repo;

    public UsuarioService(UsuarioRepository repo)
    {
        _repo = repo;
    }

    public Task<List<Usuario>> ObtenerTodos() => _repo.ObtenerTodos();

    public Task<Usuario?> ObtenerPorId(string id) => _repo.ObtenerPorId(id);

    public Task<Usuario?> ObtenerPorCorreo(string correo) =>
        _repo.ObtenerPorCorreo(correo.Trim().ToLower());

    public async Task<(bool ok, string? error)> Crear(Usuario usuario, string passwordPlano)
    {
        usuario.Correo = usuario.Correo.Trim().ToLower();

        var error = await Validar(usuario, null);
        if (error != null) return (false, error);

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordPlano);
        usuario.Estado = EstadosUsuario.Activo;
        usuario.FechaCreacion = DateTime.UtcNow;

        await _repo.Crear(usuario);
        return (true, null);
    }

    public async Task<(bool ok, string? error)> Actualizar(string id, Usuario usuario)
    {
        var existente = await _repo.ObtenerPorId(id);
        if (existente == null) return (false, "Usuario no encontrado.");

        usuario.Id = id;
        usuario.Correo = usuario.Correo.Trim().ToLower();

        var error = await Validar(usuario, id);
        if (error != null) return (false, error);

        await _repo.Actualizar(usuario);
        return (true, null);
    }

    public async Task<(bool ok, string? error)> Inactivar(string id)
    {
        var existente = await _repo.ObtenerPorId(id);
        if (existente == null) return (false, "Usuario no encontrado.");

        await _repo.CambiarEstado(id, EstadosUsuario.Inactivo);
        return (true, null);
    }

    private async Task<string?> Validar(Usuario u, string? idActual)
    {
        if (u.Rol is not (Roles.Empleado or Roles.Jefatura or Roles.RRHH))
            return "Rol inválido.";

        var porCorreo = await _repo.ObtenerPorCorreo(u.Correo);
        if (porCorreo != null && porCorreo.Id != idActual)
            return "Ya existe un usuario con ese correo.";

        var porIdent = await _repo.ObtenerPorIdentificacion(u.Identificacion);
        if (porIdent != null && porIdent.Id != idActual)
            return "Ya existe un usuario con esa identificación.";

        if (u.JefaturaDirectaId != null)
        {
            if (u.JefaturaDirectaId == idActual)
                return "Un empleado no puede ser su propia jefatura.";

            var jefe = await _repo.ObtenerPorId(u.JefaturaDirectaId);
            if (jefe == null) return "La jefatura indicada no existe.";
        }

        return null;
    }
}