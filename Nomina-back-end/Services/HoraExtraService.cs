using System.Globalization;
using Nomina_back_end.DTOs;
using Nomina_back_end.Entities;
using Nomina_back_end.Repositories;

namespace Nomina_back_end.Services;

public class HoraExtraService
{
    private const string ClaveDias = "planilla.dias_por_mes";
    private const string ClaveHoras = "planilla.horas_por_dia";

    private readonly HoraExtraRepository _repo;
    private readonly UsuarioRepository _usuarios;
    private readonly TipoHoraExtraRepository _tipos;
    private readonly AsistenciaRepository _asistencias;
    private readonly FeriadoRepository _feriados;
    private readonly DepartamentoRepository _departamentos;
    private readonly ParametroService _parametros;

    public HoraExtraService(HoraExtraRepository repo, UsuarioRepository usuarios,
        TipoHoraExtraRepository tipos, AsistenciaRepository asistencias,
        FeriadoRepository feriados, DepartamentoRepository departamentos, ParametroService parametros)
    {
        _repo = repo;
        _usuarios = usuarios;
        _tipos = tipos;
        _asistencias = asistencias;
        _feriados = feriados;
        _departamentos = departamentos;
        _parametros = parametros;
    }

    private record Calculo(decimal ValorHora, decimal Recargo, decimal Monto);
    private record Datos(DateTime Inicio, DateTime Fin, decimal Horas, Calculo Calculo);

    // ---------- Crear / editar / cancelar ----------

    public async Task<(Resultado r, HoraExtraRespuestaDto? h)> Crear(string usuarioId, HoraExtraGuardarDto dto)
    {
        var u = await _usuarios.ObtenerPorId(usuarioId);
        if (u == null || u.Estado != EstadosUsuario.Activo)
            return (Resultado.Fallo("Usuario no válido.", 403), null);

        var (r, d) = await Validar(u, dto, null);
        if (!r.Ok) return (r, null);

        var ahora = DateTime.UtcNow;
        var s = new SolicitudHoraExtra
        {
            UsuarioId = usuarioId,
            TipoHoraExtraId = dto.TipoHoraExtraId,
            Fecha = dto.Fecha,
            Inicio = d!.Inicio,
            Fin = d.Fin,
            Horas = d.Horas,
            Justificacion = dto.Justificacion.Trim(),
            ValorHora = d.Calculo.ValorHora,
            Recargo = d.Calculo.Recargo,
            Monto = d.Calculo.Monto,
            FechaCreacion = ahora,
            Historial =
            {
                new CambioEstado { Estado = EstadosSolicitud.Pendiente, UsuarioId = usuarioId, Fecha = ahora }
            }
        };

        await _repo.Crear(s);
        return (Resultado.Exito(), (await Mapear(new[] { s }))[0]);
    }

    public async Task<(Resultado r, HoraExtraRespuestaDto? h)> Editar(
        string id, string usuarioId, HoraExtraGuardarDto dto)
    {
        var s = await _repo.ObtenerPorId(id);
        if (s == null) return (Resultado.Fallo("Solicitud no encontrada.", 404), null);
        if (s.UsuarioId != usuarioId)
            return (Resultado.Fallo("Solo puedes editar tus propias solicitudes.", 403), null);
        if (s.Estado != EstadosSolicitud.Pendiente)
            return (Resultado.Fallo("Solo se pueden editar solicitudes pendientes.", 409), null);

        var u = await _usuarios.ObtenerPorId(usuarioId);
        var (r, d) = await Validar(u!, dto, id);
        if (!r.Ok) return (r, null);

        s.TipoHoraExtraId = dto.TipoHoraExtraId;
        s.Fecha = dto.Fecha;
        s.Inicio = d!.Inicio;
        s.Fin = d.Fin;
        s.Horas = d.Horas;
        s.Justificacion = dto.Justificacion.Trim();
        s.ValorHora = d.Calculo.ValorHora;
        s.Recargo = d.Calculo.Recargo;
        s.Monto = d.Calculo.Monto;

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

    // ---------- Aprobar / rechazar ----------

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
        var cambio = new CambioEstado { Estado = nuevoEstado, UsuarioId = jefeId, Observacion = obs };

        Calculo? calculo = null;

        if (nuevoEstado == EstadosSolicitud.Aprobada)
        {
            // Las horas pedidas deben estar respaldadas por la asistencia
            var a = await _asistencias.ObtenerPorUsuarioYFecha(s.UsuarioId, s.Fecha);

            if (a?.HoraEntrada == null || a.HoraSalida == null)
                return Resultado.Fallo(
                    "No hay asistencia completa (entrada y salida) para esa fecha: las horas no se pueden validar.");

            if (s.Inicio < a.HoraEntrada || s.Fin > a.HoraSalida)
                return Resultado.Fallo(
                    "Las horas solicitadas exceden las horas realmente laboradas según la asistencia " +
                    $"({HoraCR.ALocal(a.HoraEntrada.Value):HH:mm} a {HoraCR.ALocal(a.HoraSalida.Value):HH:mm}).");

            // Se recalcula con el salario y el recargo vigentes y queda guardado
            var tipo = await _tipos.ObtenerPorId(s.TipoHoraExtraId);
            if (tipo == null) return Resultado.Fallo("El tipo de hora extra ya no existe.");

            var (rc, c) = await CalcularMonto(empleado, tipo, s.Horas);
            if (!rc.Ok) return rc;
            calculo = c;
        }

        var ok = await _repo.CambiarEstadoSiPendiente(id, nuevoEstado, jefeId, obs, cambio,
            calculo?.ValorHora, calculo?.Recargo, calculo?.Monto);

        return ok
            ? Resultado.Exito()
            : Resultado.Fallo("La solicitud ya fue resuelta y no puede cambiar de estado.", 409);
    }

    // ---------- Consultas ----------

    public async Task<List<HoraExtraRespuestaDto>> Listar(
        string solicitanteId, string rol, string? usuarioId, string? departamentoId,
        string? estado, string? tipoId, string? desde, string? hasta)
    {
        var universo = await Universo(solicitanteId, rol, usuarioId, departamentoId);
        if (universo.Count == 0) return new();

        var lista = await _repo.Buscar(universo.Select(u => u.Id!), estado, tipoId,
            FechaOpcional(desde), FechaOpcional(hasta));

        return await Mapear(lista);
    }

    public async Task<(Resultado r, HoraExtraRespuestaDto? h)> ObtenerPorId(string id, string solicitanteId, string rol)
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

    // ---------- Reporte (solo horas extra aprobadas) ----------

    public async Task<ReporteHorasExtraDto> Reporte(
        string solicitanteId, string rol, string? usuarioId, string? departamentoId, string? desde, string? hasta)
    {
        var universo = await Universo(solicitanteId, rol, usuarioId, departamentoId);
        var d = FechaOpcional(desde);
        var h = FechaOpcional(hasta);

        var reporte = new ReporteHorasExtraDto { Desde = d, Hasta = h };
        if (universo.Count == 0) return reporte;

        var registros = await _repo.Buscar(universo.Select(u => u.Id!), EstadosSolicitud.Aprobada, null, d, h);
        var usuarios = universo.ToDictionary(u => u.Id!);
        var departamentos = (await _departamentos.ObtenerTodos()).ToDictionary(x => x.Id!, x => x.Nombre);

        reporte.PorEmpleado = registros
            .GroupBy(r => r.UsuarioId)
            .Select(g => new ReporteLineaDto
            {
                Id = g.Key,
                Nombre = usuarios[g.Key].Nombre,
                Horas = g.Sum(x => x.Horas),
                Monto = g.Sum(x => x.Monto),
                Solicitudes = g.Count()
            })
            .OrderByDescending(x => x.Monto)
            .ToList();

        reporte.PorDepartamento = registros
            .GroupBy(r => usuarios[r.UsuarioId].DepartamentoId)
            .Select(g => new ReporteLineaDto
            {
                Id = g.Key,
                Nombre = departamentos.GetValueOrDefault(g.Key, "Sin departamento"),
                Horas = g.Sum(x => x.Horas),
                Monto = g.Sum(x => x.Monto),
                Solicitudes = g.Count()
            })
            .OrderByDescending(x => x.Monto)
            .ToList();

        reporte.TotalHoras = registros.Sum(x => x.Horas);
        reporte.TotalMonto = registros.Sum(x => x.Monto);
        return reporte;
    }

    // ---------- Internos ----------

    private async Task<List<Usuario>> Universo(
        string solicitanteId, string rol, string? usuarioId, string? departamentoId)
    {
        var todos = await _usuarios.ObtenerTodos();

        IEnumerable<Usuario> universo = rol switch
        {
            Roles.RRHH => todos,
            Roles.Jefatura => todos.Where(u => u.Id == solicitanteId || u.JefaturaDirectaId == solicitanteId),
            _ => todos.Where(u => u.Id == solicitanteId)
        };

        if (!string.IsNullOrWhiteSpace(usuarioId))
            universo = universo.Where(u => u.Id == usuarioId);
        if (!string.IsNullOrWhiteSpace(departamentoId))
            universo = universo.Where(u => u.DepartamentoId == departamentoId);

        return universo.ToList();
    }

    private async Task<(Resultado r, Datos? d)> Validar(Usuario u, HoraExtraGuardarDto dto, string? excluirId)
    {
        var tipo = await _tipos.ObtenerPorId(dto.TipoHoraExtraId);
        if (tipo == null) return (Resultado.Fallo("El tipo de hora extra no existe."), null);

        if (!FechaValida(dto.Fecha)) return (Resultado.Fallo("Fecha inválida."), null);

        var inicio = HoraCR.AUtc(dto.Fecha, dto.HoraInicio);
        var fin = HoraCR.AUtc(dto.Fecha, dto.HoraFin);

        if (fin <= inicio)
            return (Resultado.Fallo("La hora de fin debe ser posterior a la de inicio (dentro del mismo día)."), null);

        // Las horas extra no pueden coincidir con la jornada ordinaria
        var fecha = DateTime.ParseExact(dto.Fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        var esFeriado = await _feriados.ObtenerPorFecha(dto.Fecha) != null;

        if (!esFeriado && u.Horario.DiasLaborales.Contains(HoraCR.DiaSemana(fecha)))
        {
            var ordinariaIni = HoraCR.AUtc(dto.Fecha, u.Horario.HoraEntrada);
            var ordinariaFin = HoraCR.AUtc(dto.Fecha, u.Horario.HoraSalida);

            if (inicio < ordinariaFin && fin > ordinariaIni)
                return (Resultado.Fallo(
                    $"Las horas extra no pueden coincidir con la jornada ordinaria ({u.Horario.HoraEntrada} a {u.Horario.HoraSalida})."),
                    null);
        }

        if (await _repo.ExisteTraslape(u.Id!, inicio, fin, excluirId))
            return (Resultado.Fallo("Ya tienes una solicitud de horas extra en ese rango.", 409), null);

        var horas = Math.Round((decimal)(fin - inicio).TotalHours, 2);

        var (rc, calculo) = await CalcularMonto(u, tipo, horas);
        if (!rc.Ok) return (rc, null);

        return (Resultado.Exito(), new Datos(inicio, fin, horas, calculo!));
    }

    private async Task<(Resultado r, Calculo? c)> CalcularMonto(Usuario u, TipoHoraExtra tipo, decimal horas)
    {
        var dias = await _parametros.ObtenerValor(ClaveDias);
        var horasDia = await _parametros.ObtenerValor(ClaveHoras);

        if (!dias.HasValue || dias <= 0 || !horasDia.HasValue || horasDia <= 0)
            return (Resultado.Fallo($"RR.HH. debe configurar los parámetros '{ClaveDias}' y '{ClaveHoras}'."), null);

        var valorHora = Math.Round(u.SalarioBase / dias.Value / horasDia.Value, 4);
        var monto = Math.Round(valorHora * horas * tipo.Recargo, 2);

        return (Resultado.Exito(), new Calculo(valorHora, tipo.Recargo, monto));
    }

    private static bool FechaValida(string? fecha) =>
        fecha != null &&
        DateTime.TryParseExact(fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

    private static string? FechaOpcional(string? fecha) => FechaValida(fecha) ? fecha : null;

    private async Task<List<HoraExtraRespuestaDto>> Mapear(IEnumerable<SolicitudHoraExtra> lista)
    {
        var usuarios = (await _usuarios.ObtenerTodos()).ToDictionary(u => u.Id!);
        var tipos = (await _tipos.ObtenerTodos()).ToDictionary(t => t.Id!, t => t.Nombre);

        return lista.Select(s =>
        {
            usuarios.TryGetValue(s.UsuarioId, out var emp);
            Usuario? apr = null;
            if (s.AprobadorId != null) usuarios.TryGetValue(s.AprobadorId, out apr);

            return new HoraExtraRespuestaDto
            {
                Id = s.Id,
                UsuarioId = s.UsuarioId,
                NombreEmpleado = emp?.Nombre ?? string.Empty,
                FotoUrl = emp?.FotoUrl,
                TipoHoraExtraId = s.TipoHoraExtraId,
                TipoHoraExtra = tipos.GetValueOrDefault(s.TipoHoraExtraId, string.Empty),
                Fecha = s.Fecha,
                HoraInicio = HoraCR.ALocal(s.Inicio).ToString("HH:mm"),
                HoraFin = HoraCR.ALocal(s.Fin).ToString("HH:mm"),
                Horas = s.Horas,
                Justificacion = s.Justificacion,
                ValorHora = s.ValorHora,
                Recargo = s.Recargo,
                Monto = s.Monto,
                Estado = s.Estado,
                AprobadorId = s.AprobadorId,
                AprobadorNombre = apr?.Nombre,
                Observacion = s.Observacion,
                FechaResolucion = s.FechaResolucion
            };
        }).ToList();
    }
}