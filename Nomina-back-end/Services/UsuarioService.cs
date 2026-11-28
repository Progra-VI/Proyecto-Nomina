using Nomina_back_end.Entities;
using Nomina_back_end.Repositories;

namespace Nomina_back_end.Services;

public class UsuarioService
{

    private readonly UsuarioRepository _repo;
    private readonly DepartamentoRepository _departamentos;

    public UsuarioService(UsuarioRepository repo, DepartamentoRepository departamentos)
    {
        _repo = repo;
        _departamentos = departamentos;
    }

    public Task<List<Usuario>> ObtenerTodos() => _repo.ObtenerTodos();

    public Task<Usuario?> ObtenerPorId(string id) => _repo.ObtenerPorId(id);

    public Task<List<Usuario>> ObtenerJefaturasActivas() =>
    _repo.ObtenerJefaturasActivas();

    public Task<Usuario?> ObtenerPorCorreo(string correo) =>
        _repo.ObtenerPorCorreo(correo.Trim().ToLower());

    public async Task<(bool ok, string? error)> Crear(Usuario usuario, string passwordPlano)
    {
        usuario.Correo = usuario.Correo.Trim().ToLower();
        var identificacion = NormalizarIdentificacion(usuario.Identificacion);

        if (identificacion == null)
            return (false, "La identificación debe tener el formato #-####-####.");

        usuario.Identificacion = identificacion;

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
        var identificacion = NormalizarIdentificacion(usuario.Identificacion);

        if (identificacion == null)
            return (false, "La identificación debe tener el formato #-####-####.");

        usuario.Identificacion = identificacion;

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

        var departamento = await _departamentos.ObtenerPorId(u.DepartamentoId);

        if (departamento == null)
            return "El departamento indicado no existe.";

        var porCorreo = await _repo.ObtenerPorCorreo(u.Correo);

        if (porCorreo != null && porCorreo.Id != idActual)
            return "Ya existe un usuario con ese correo.";

        var porIdent = await _repo.ObtenerPorIdentificacion(u.Identificacion);

        if (porIdent != null && porIdent.Id != idActual)
            return "Ya existe un usuario con esa identificación.";

        // Un empleado debe tener una jefatura directa.
        if (u.Rol == Roles.Empleado &&
            string.IsNullOrWhiteSpace(u.JefaturaDirectaId))
        {
            return "El empleado debe tener una jefatura directa.";
        }

        // Validar la jefatura seleccionada, si existe.
        if (!string.IsNullOrWhiteSpace(u.JefaturaDirectaId))
        {
            if (u.JefaturaDirectaId == idActual)
                return "Un usuario no puede ser su propia jefatura.";

            var jefe = await _repo.ObtenerPorId(u.JefaturaDirectaId);

            if (jefe == null)
                return "La jefatura indicada no existe.";

            if (jefe.Rol != Roles.Jefatura)
                return "El usuario seleccionado debe tener el rol Jefatura.";

            if (jefe.Estado != EstadosUsuario.Activo)
                return "La jefatura seleccionada está inactiva.";

            // Los empleados deben depender de una jefatura
            // perteneciente a su mismo departamento.
            if (u.Rol == Roles.Empleado &&
                jefe.DepartamentoId != u.DepartamentoId)
            {
                return "La jefatura debe pertenecer al mismo departamento del empleado.";
            }

            // Evitar ciclos en la jerarquía.
            var jefeId = jefe.Id;
            var visitados = new HashSet<string>();

            while (!string.IsNullOrWhiteSpace(jefeId))
            {
                if (jefeId == idActual)
                    return "Esta asignación generaría un ciclo en la jerarquía.";

                if (!visitados.Add(jefeId))
                    return "Se detectó una relación circular en la jerarquía existente.";

                var superior = await _repo.ObtenerPorId(jefeId);

                if (superior == null)
                    break;

                jefeId = superior.JefaturaDirectaId;
            }
        }

        return null;
    }

    public async Task<Resultado> CambiarPassword(string usuarioId, string actual, string nueva)
    {
        var u = await _repo.ObtenerPorId(usuarioId);
        if (u == null || u.Estado != EstadosUsuario.Activo)
            return Resultado.Fallo("Usuario no válido.", 403);

        // Es 400 y no 401 a propósito: un 401 haría que el frontend cierre la sesión
        if (!BCrypt.Net.BCrypt.Verify(actual, u.PasswordHash))
            return Resultado.Fallo("La contraseña actual es incorrecta.");

        if (actual == nueva)
            return Resultado.Fallo("La nueva contraseña debe ser distinta a la actual.");

        await _repo.CambiarPassword(usuarioId, BCrypt.Net.BCrypt.HashPassword(nueva));
        return Resultado.Exito();
    }

    public async Task<Resultado> RestablecerPassword(string id, string nueva)
    {
        if (await _repo.ObtenerPorId(id) == null)
            return Resultado.Fallo("Usuario no encontrado.", 404);

        await _repo.CambiarPassword(id, BCrypt.Net.BCrypt.HashPassword(nueva));
        return Resultado.Exito();
    }

    public async Task<List<Usuario>> ObtenerEquipo(string jefeId) =>
      (await _repo.ObtenerPorJefatura(jefeId))
          .Where(u => u.Estado == EstadosUsuario.Activo)
          .OrderBy(u => u.Nombre)
          .ToList();

    private static string? NormalizarIdentificacion(string identificacion)
    {
        if (string.IsNullOrWhiteSpace(identificacion))
            return null;

        var valor = identificacion.Trim();

        if (!System.Text.RegularExpressions.Regex.IsMatch(
            valor, @"^\d{1}-?\d{4}-?\d{4}$"))
            return null;

        var digitos = valor.Replace("-", "");

        return $"{digitos[0]}-{digitos.Substring(1, 4)}-{digitos.Substring(5, 4)}";
    }
}