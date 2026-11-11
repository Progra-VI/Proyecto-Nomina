using System.Globalization;
using MongoDB.Bson;
using Nomina_back_end.DTOs;
using Nomina_back_end.Entities;
using Nomina_back_end.Repositories;

namespace Nomina_back_end.Services;

public class PermisoService
{
    private readonly PermisoRepository _repo;
    private readonly UsuarioRepository _usuarios;
    private readonly TipoPermisoRepository _tipos;

    private readonly VacacionRepository _vacaciones;

    public PermisoService(PermisoRepository repo, UsuarioRepository usuarios,
        TipoPermisoRepository tipos, VacacionRepository vacaciones)
    {
        _repo = repo;
        _usuarios = usuarios;
        _tipos = tipos;
        _vacaciones = vacaciones;
    }

    public PermisoService(PermisoRepository repo, UsuarioRepository usuarios, TipoPermisoRepository tipos)
    {
        _repo = repo;
        _usuarios = usuarios;
        _tipos = tipos;
    }

    // ---------- Crear / editar / cancelar (solicitante) ----------

    public async Task<(Resultado r, PermisoRespuestaDto? permiso)> Crear(string usuarioId, PermisoGuardarDto dto)
    {
        var usuario = await _usuarios.ObtenerPorId(usuarioId);
        if (usuario == null || usuario.Estado != EstadosUsuario.Activo)
            return (Resultado.Fallo("Usuario no válido.", 403), null);

        var (r, inicio, fin) = await Validar(dto, usuarioId, null);
        if (!r.Ok) return (r, null);

        var ahora = DateTime.UtcNow;
        var s = new SolicitudPermiso
        {
            UsuarioId = usuarioId,
            TipoPermisoId = dto.TipoPermisoId,
            FechaInicio = inicio,
            FechaFin = fin,
            Motivo = dto.Motivo.Trim(),
            ConGoce = dto.ConGoce,
            Estado = EstadosSolicitud.Pendiente,
            FechaCreacion = ahora,
            Historial =
            {
                new CambioEstado { Estado = EstadosSolicitud.Pendiente, UsuarioId = usuarioId, Fecha = ahora }
            }
        };

        await _repo.Crear(s);
        return (Resultado.Exito(), (await Mapear(new[] { s }))[0]);
    }

    public async Task<(Resultado r, PermisoRespuestaDto? permiso)> Editar(
        string id, string usuarioId, PermisoGuardarDto dto)
    {
        var s = await _repo.ObtenerPorId(id);
        if (s == null) return (Resultado.Fallo("Solicitud no encontrada.", 404), null);
        if (s.UsuarioId != usuarioId)
            return (Resultado.Fallo("Solo puedes editar tus propias solicitudes.", 403), null);
        if (s.Estado != EstadosSolicitud.Pendiente)
            return (Resultado.Fallo("Solo se pueden editar solicitudes pendientes.", 409), null);

        var (r, inicio, fin) = await Validar(dto, usuarioId, id);
        if (!r.Ok) return (r, null);

        s.TipoPermisoId = dto.TipoPermisoId;
        s.FechaInicio = inicio;
        s.FechaFin = fin;
        s.Motivo = dto.Motivo.Trim();
        s.ConGoce = dto.ConGoce;

        if (!await _repo.ActualizarSiPendiente(s))
            return (Resultado.Fallo("La solicitud ya fue resuelta y no puede modificarse.", 409), null);

        return (Resultado.Exito(), (await Mapear(new[] { s }))[0]);
    }

    public async Task<Resultado> Cancelar(string id, string usuarioId)
    {
        var s = await _repo.ObtenerPorId(id);
        if (s == null) return Resultado.Fallo("Solicitud no encontrada.", 404);
        if (s.UsuarioId != usuarioId)
            return Resultado.Fallo("Solo puedes cancelar tus propias solicitudes.", 403);
        if (s.Estado != EstadosSolicitud.Pendiente)
            return Resultado.Fallo("Solo se pueden cancelar solicitudes pendientes.", 409);

        var cambio = new CambioEstado { Estado = EstadosSolicitud.Cancelada, UsuarioId = usuarioId };

        return await _repo.CambiarEstadoSiPendiente(id, EstadosSolicitud.Cancelada, null, null, cambio)
            ? Resultado.Exito()
            : Resultado.Fallo("La solicitud ya fue resuelta.", 409);
    }

    // ---------- Aprobar / rechazar (jefatura directa) ----------

    public async Task<Resultado> Resolver(string id, string jefeId, string nuevoEstado, string? observacion)
    {
        var s = await _repo.ObtenerPorId(id);
        if (s == null) return Resultado.Fallo("Solicitud no encontrada.", 404);

        var empleado = await _usuarios.ObtenerPorId(s.UsuarioId);
        if (empleado?.JefaturaDirectaId != jefeId)
            return Resultado.Fallo("Solo la jefatura directa puede resolver esta solicitud.", 403);

        if (nuevoEstado == EstadosSolicitud.Rechazada && string.IsNullOrWhiteSpace(observacion))
            return Resultado.Fallo("La observación es obligatoria al rechazar.");

        if (s.Estado != EstadosSolicitud.Pendiente)
            return Resultado.Fallo("La solicitud ya fue resuelta y no puede cambiar de estado.", 409);

        var obs = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim();
        var cambio = new CambioEstado
        {
            Estado = nuevoEstado,
            UsuarioId = jefeId,
            Observacion = obs
        };

        return await _repo.CambiarEstadoSiPendiente(id, nuevoEstado, jefeId, obs, cambio)
            ? Resultado.Exito()
            : Resultado.Fallo("La solicitud ya fue resuelta y no puede cambiar de estado.", 409);
    }

    // ---------- Consultas ----------

    public async Task<List<PermisoRespuestaDto>> Listar(
        string solicitanteId, string rol,
        string? usuarioId, string? estado, string? tipoId, string? desde, string? hasta)
    {
        if (!string.IsNullOrWhiteSpace(tipoId) && !ObjectId.TryParse(tipoId, out _))
            return new();

        var todos = await _usuarios.ObtenerTodos();

        IEnumerable<Usuario> universo = rol switch
        {
            Roles.RRHH => todos,
            Roles.Jefatura => todos.Where(u => u.Id == solicitanteId || u.JefaturaDirectaId == solicitanteId),
            _ => todos.Where(u => u.Id == solicitanteId)
        };

        if (!string.IsNullOrWhiteSpace(usuarioId))
            universo = universo.Where(u => u.Id == usuarioId);

        var ids = universo.Select(u => u.Id!).ToList();
        if (ids.Count == 0) return new();

        DateTime? d = FechaValida(desde) ? HoraCR.AUtc(desde!, "00:00") : null;
        DateTime? h = FechaValida(hasta) ? HoraCR.AUtc(hasta!, "23:59") : null;

        var lista = await _repo.Buscar(ids, estado, tipoId, d, h);
        return await Mapear(lista);
    }

    public async Task<(Resultado r, PermisoRespuestaDto? permiso)> ObtenerPorId(
        string id, string solicitanteId, string rol)
    {
        var s = await _repo.ObtenerPorId(id);
        if (s == null) return (Resultado.Fallo("Solicitud no encontrada.", 404), null);

        if (rol != Roles.RRHH && s.UsuarioId != solicitanteId)
        {
            var empleado = await _usuarios.ObtenerPorId(s.UsuarioId);
            if (empleado?.JefaturaDirectaId != solicitanteId)
                return (Resultado.Fallo("No tienes acceso a esta solicitud.", 403), null);
        }

        return (Resultado.Exito(), (await Mapear(new[] { s }))[0]);
    }

    // ---------- Internos ----------

    private async Task<(Resultado r, DateTime inicio, DateTime fin)> Validar(
        PermisoGuardarDto dto, string usuarioId, string? excluirId)
    {
        if (await _tipos.ObtenerPorId(dto.TipoPermisoId) == null)
            return (Resultado.Fallo("El tipo de permiso no existe."), default, default);

        if (!FechaValida(dto.FechaInicio) || !FechaValida(dto.FechaFin))
            return (Resultado.Fallo("Fecha inválida."), default, default);

        var inicio = HoraCR.AUtc(dto.FechaInicio, dto.HoraInicio);
        var fin = HoraCR.AUtc(dto.FechaFin, dto.HoraFin);

        if (fin <= inicio)
            return (Resultado.Fallo("La fecha y hora de fin debe ser posterior al inicio."), default, default);

        if (await _repo.ExisteTraslape(usuarioId, inicio, fin, excluirId))
            return (Resultado.Fallo("Ya tienes una solicitud en ese rango de fechas.", 409), default, default);

        if (await _vacaciones.ExisteTraslape(usuarioId, inicio, fin, null))
            return (Resultado.Fallo("Ya tienes vacaciones en ese rango de fechas.", 409), default, default);

        return (Resultado.Exito(), inicio, fin);
    }

    private static bool FechaValida(string? fecha) =>
        fecha != null &&
        DateTime.TryParseExact(fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

    private async Task<List<PermisoRespuestaDto>> Mapear(IEnumerable<SolicitudPermiso> lista)
    {
        var usuarios = (await _usuarios.ObtenerTodos()).ToDictionary(u => u.Id!);
        var tipos = (await _tipos.ObtenerTodos()).ToDictionary(t => t.Id!, t => t.Nombre);

        return lista.Select(s =>
        {
            usuarios.TryGetValue(s.UsuarioId, out var emp);
            Usuario? apr = null;
            if (s.AprobadorId != null) usuarios.TryGetValue(s.AprobadorId, out apr);

            var ini = HoraCR.ALocal(s.FechaInicio);
            var fin = HoraCR.ALocal(s.FechaFin);

            return new PermisoRespuestaDto
            {
                Id = s.Id,
                UsuarioId = s.UsuarioId,
                NombreEmpleado = emp?.Nombre ?? string.Empty,
                FotoUrl = emp?.FotoUrl,
                TipoPermisoId = s.TipoPermisoId,
                TipoPermiso = tipos.GetValueOrDefault(s.TipoPermisoId, string.Empty),
                FechaInicio = ini.ToString("yyyy-MM-dd"),
                HoraInicio = ini.ToString("HH:mm"),
                FechaFin = fin.ToString("yyyy-MM-dd"),
                HoraFin = fin.ToString("HH:mm"),
                Motivo = s.Motivo,
                ConGoce = s.ConGoce,
                Estado = s.Estado,
                AprobadorId = s.AprobadorId,
                AprobadorNombre = apr?.Nombre,
                Observacion = s.Observacion,
                FechaResolucion = s.FechaResolucion
            };
        }).ToList();
    }
}