using System.Globalization;
using Nomina_back_end.DTOs;
using Nomina_back_end.Entities;
using Nomina_back_end.Repositories;

namespace Nomina_back_end.Services;

public class VacacionService
{
    private const string ClaveSemanas = "vacaciones.semanas_por_periodo";
    private const string ClaveDias = "vacaciones.dias_por_periodo";

    private readonly VacacionRepository _repo;
    private readonly UsuarioRepository _usuarios;
    private readonly PermisoRepository _permisos;
    private readonly FeriadoRepository _feriados;
    private readonly ParametroService _parametros;

    public VacacionService(VacacionRepository repo, UsuarioRepository usuarios,
        PermisoRepository permisos, FeriadoRepository feriados, ParametroService parametros)
    {
        _repo = repo;
        _usuarios = usuarios;
        _permisos = permisos;
        _feriados = feriados;
        _parametros = parametros;
    }

    private record Calculo(decimal Acumulado, decimal Aprobado, decimal Pendiente);

    // ---------- Saldo ----------

    public async Task<(Resultado r, SaldoVacacionesDto? saldo)> ConsultarSaldo(string usuarioId)
    {
        var u = await _usuarios.ObtenerPorId(usuarioId);
        if (u == null) return (Resultado.Fallo("Empleado no encontrado.", 404), null);

        var (r, calc) = await CalcularSaldo(u, null);
        if (!r.Ok) return (r, null);

        var movimientos = await Mapear(await _repo.ObtenerPorUsuario(usuarioId));
        var saldo = calc!.Acumulado - calc.Aprobado;

        return (Resultado.Exito(), new SaldoVacacionesDto
        {
            UsuarioId = usuarioId,
            NombreEmpleado = u.Nombre,
            FechaIngreso = u.FechaIngreso.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Acumulado = calc.Acumulado,
            Disfrutado = calc.Aprobado,
            Pendiente = calc.Pendiente,
            Saldo = saldo,
            Disponible = saldo - calc.Pendiente,
            Movimientos = movimientos
        });
    }

    // Reutilizable por la planilla y la liquidación
    public async Task<(Resultado r, decimal saldo)> SaldoActual(string usuarioId)
    {
        var u = await _usuarios.ObtenerPorId(usuarioId);
        if (u == null) return (Resultado.Fallo("Empleado no encontrado.", 404), 0);

        var (r, calc) = await CalcularSaldo(u, null);
        return r.Ok ? (r, calc!.Acumulado - calc.Aprobado) : (r, 0);
    }

    private async Task<(Resultado r, Calculo? calc)> CalcularSaldo(Usuario u, string? excluirId)
    {
        var semanasPorPeriodo = await _parametros.ObtenerValor(ClaveSemanas);
        var diasPorPeriodo = await _parametros.ObtenerValor(ClaveDias);

        if (semanasPorPeriodo is null or <= 0 || diasPorPeriodo is null)
            return (Resultado.Fallo(
                $"RR.HH. debe configurar los parámetros '{ClaveSemanas}' y '{ClaveDias}'.", 400), null);

        var hoy = HoraCR.ALocal(DateTime.UtcNow).Date;
        var semanas = Math.Max(0, Math.Floor((hoy - u.FechaIngreso.Date).TotalDays / 7));
        var acumulado = Math.Round((decimal)semanas * diasPorPeriodo.Value / semanasPorPeriodo.Value, 2);

        var solicitudes = await _repo.ObtenerPorUsuario(u.Id!);
        var aprobado = solicitudes.Where(s => s.Estado == EstadosSolicitud.Aprobada).Sum(s => s.Dias);
        var pendiente = solicitudes
            .Where(s => s.Estado == EstadosSolicitud.Pendiente && s.Id != excluirId)
            .Sum(s => s.Dias);

        return (Resultado.Exito(), new Calculo(acumulado, aprobado, pendiente));
    }

    // ---------- Crear / editar / cancelar ----------

    public async Task<(Resultado r, VacacionRespuestaDto? v)> Crear(string usuarioId, VacacionGuardarDto dto)
    {
        var u = await _usuarios.ObtenerPorId(usuarioId);
        if (u == null || u.Estado != EstadosUsuario.Activo)
            return (Resultado.Fallo("Usuario no válido.", 403), null);

        var (r, inicio, fin, dias) = await Validar(u, dto, null);
        if (!r.Ok) return (r, null);

        var ahora = DateTime.UtcNow;
        var s = new SolicitudVacaciones
        {
            UsuarioId = usuarioId,
            FechaInicio = inicio,
            FechaFin = fin,
            Dias = dias,
            Motivo = string.IsNullOrWhiteSpace(dto.Motivo) ? null : dto.Motivo.Trim(),
            FechaCreacion = ahora,
            Historial =
            {
                new CambioEstado { Estado = EstadosSolicitud.Pendiente, UsuarioId = usuarioId, Fecha = ahora }
            }
        };

        await _repo.Crear(s);
        return (Resultado.Exito(), (await Mapear(new[] { s }))[0]);
    }

    public async Task<(Resultado r, VacacionRespuestaDto? v)> Editar(
        string id, string usuarioId, VacacionGuardarDto dto)
    {
        var s = await _repo.ObtenerPorId(id);
        if (s == null) return (Resultado.Fallo("Solicitud no encontrada.", 404), null);
        if (s.UsuarioId != usuarioId)
            return (Resultado.Fallo("Solo puedes editar tus propias solicitudes.", 403), null);
        if (s.Estado != EstadosSolicitud.Pendiente)
            return (Resultado.Fallo("Solo se pueden editar solicitudes pendientes.", 409), null);

        var u = await _usuarios.ObtenerPorId(usuarioId);
        var (r, inicio, fin, dias) = await Validar(u!, dto, id);
        if (!r.Ok) return (r, null);

        s.FechaInicio = inicio;
        s.FechaFin = fin;
        s.Dias = dias;
        s.Motivo = string.IsNullOrWhiteSpace(dto.Motivo) ? null : dto.Motivo.Trim();

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

        // Al pasar a Aprobada el saldo baja solo, porque se calcula con las aprobadas
        return await _repo.CambiarEstadoSiPendiente(id, nuevoEstado, jefeId, obs, cambio)
            ? Resultado.Exito()
            : Resultado.Fallo("La solicitud ya fue resuelta y no puede cambiar de estado.", 409);
    }

    // ---------- Consultas ----------

    public async Task<List<VacacionRespuestaDto>> Listar(
        string solicitanteId, string rol, string? usuarioId, string? estado, string? desde, string? hasta)
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

        var ids = universo.Select(u => u.Id!).ToList();
        if (ids.Count == 0) return new();

        DateTime? d = FechaValida(desde) ? HoraCR.AUtc(desde!, "00:00") : null;
        DateTime? h = FechaValida(hasta) ? HoraCR.AUtc(hasta!, "23:59") : null;

        return await Mapear(await _repo.Buscar(ids, estado, d, h));
    }

    public async Task<(Resultado r, VacacionRespuestaDto? v)> ObtenerPorId(string id, string solicitanteId, string rol)
    {
        var s = await _repo.ObtenerPorId(id);
        if (s == null) return (Resultado.Fallo("Solicitud no encontrada.", 404), null);

        if (!await PuedeConsultar(solicitanteId, rol, s.UsuarioId))
            return (Resultado.Fallo("No tienes acceso a esta solicitud.", 403), null);

        return (Resultado.Exito(), (await Mapear(new[] { s }))[0]);
    }

    public async Task<bool> PuedeConsultar(string solicitanteId, string rol, string objetivoId)
    {
        if (rol == Roles.RRHH || solicitanteId == objetivoId) return true;

        var objetivo = await _usuarios.ObtenerPorId(objetivoId);
        return objetivo?.JefaturaDirectaId == solicitanteId;
    }

    // ---------- Internos ----------

    private async Task<(Resultado r, DateTime inicio, DateTime fin, int dias)> Validar(
        Usuario u, VacacionGuardarDto dto, string? excluirId)
    {
        if (!FechaValida(dto.FechaInicio) || !FechaValida(dto.FechaFin))
            return (Resultado.Fallo("Fecha inválida."), default, default, 0);

        if (string.CompareOrdinal(dto.FechaFin, dto.FechaInicio) < 0)
            return (Resultado.Fallo("La fecha final no puede ser anterior a la inicial."), default, default, 0);

        var inicio = HoraCR.AUtc(dto.FechaInicio, "00:00");
        var fin = HoraCR.AUtc(dto.FechaFin, "00:00").AddDays(1);

        var dias = await ContarDias(u, dto.FechaInicio, dto.FechaFin);
        if (dias == 0)
            return (Resultado.Fallo("El rango no incluye días laborables."), default, default, 0);

        if (await _repo.ExisteTraslape(u.Id!, inicio, fin, excluirId))
            return (Resultado.Fallo("Ya tienes vacaciones en ese rango de fechas.", 409), default, default, 0);

        if (await _permisos.ExisteTraslape(u.Id!, inicio, fin, null))
            return (Resultado.Fallo("Ya tienes un permiso en ese rango de fechas.", 409), default, default, 0);

        var (rs, calc) = await CalcularSaldo(u, excluirId);
        if (!rs.Ok) return (rs, default, default, 0);

        var disponible = calc!.Acumulado - calc.Aprobado - calc.Pendiente;
        if (dias > disponible)
            return (Resultado.Fallo(
                $"Saldo insuficiente: solicitas {dias} día(s) y tienes {disponible} disponible(s)."),
                default, default, 0);

        return (Resultado.Exito(), inicio, fin, dias);
    }

    // Días del horario del empleado, sin feriados
    private async Task<int> ContarDias(Usuario u, string fechaInicio, string fechaFin)
    {
        var ini = DateTime.ParseExact(fechaInicio, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        var fin = DateTime.ParseExact(fechaFin, "yyyy-MM-dd", CultureInfo.InvariantCulture);

        var feriados = new HashSet<string>();
        for (var anio = ini.Year; anio <= fin.Year; anio++)
            foreach (var f in await _feriados.ObtenerPorAnio(anio))
                feriados.Add(f.Fecha);

        var dias = 0;
        for (var d = ini; d <= fin; d = d.AddDays(1))
        {
            var texto = d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            if (u.Horario.DiasLaborales.Contains(HoraCR.DiaSemana(d)) && !feriados.Contains(texto))
                dias++;
        }
        return dias;
    }

    private static bool FechaValida(string? fecha) =>
        fecha != null &&
        DateTime.TryParseExact(fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

    private async Task<List<VacacionRespuestaDto>> Mapear(IEnumerable<SolicitudVacaciones> lista)
    {
        var usuarios = (await _usuarios.ObtenerTodos()).ToDictionary(u => u.Id!);

        return lista.Select(s =>
        {
            usuarios.TryGetValue(s.UsuarioId, out var emp);
            Usuario? apr = null;
            if (s.AprobadorId != null) usuarios.TryGetValue(s.AprobadorId, out apr);

            return new VacacionRespuestaDto
            {
                Id = s.Id,
                UsuarioId = s.UsuarioId,
                NombreEmpleado = emp?.Nombre ?? string.Empty,
                FotoUrl = emp?.FotoUrl,
                FechaInicio = HoraCR.ALocal(s.FechaInicio).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                FechaFin = HoraCR.ALocal(s.FechaFin.AddDays(-1)).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Dias = s.Dias,
                Motivo = s.Motivo,
                Estado = s.Estado,
                AprobadorId = s.AprobadorId,
                AprobadorNombre = apr?.Nombre,
                Observacion = s.Observacion,
                FechaResolucion = s.FechaResolucion
            };
        }).ToList();
    }
}