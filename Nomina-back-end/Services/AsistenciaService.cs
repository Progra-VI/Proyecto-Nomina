
using System.Globalization;
using Nomina_back_end.DTOs;
using Nomina_back_end.Entities;
using Nomina_back_end.Repositories;

namespace Nomina_back_end.Services;

public class AsistenciaService
{
    private readonly AsistenciaRepository _repo;
    private readonly UsuarioRepository _usuarioRepo;

    private static readonly TimeZoneInfo ZonaCostaRica =
        ObtenerZonaCostaRica();

    public AsistenciaService(
        AsistenciaRepository repo,
        UsuarioRepository usuarioRepo)
    {
        _repo = repo;
        _usuarioRepo = usuarioRepo;
    }

    // Registrar entrada del empleado
    public async Task<(bool ok, string? error, Asistencia? asistencia)>
        RegistrarEntrada(string usuarioId)
    {
        var usuario = await _usuarioRepo.ObtenerPorId(usuarioId);

        if (usuario == null)
            return (false, "Empleado no encontrado.", null);

        if (usuario.Estado != EstadosUsuario.Activo)
            return (false, "El empleado está inactivo.", null);

        var ahoraUtc = DateTime.UtcNow;
        var ahoraLocal = TimeZoneInfo.ConvertTimeFromUtc(
            ahoraUtc, ZonaCostaRica);

        var fechaLocal = ahoraLocal.Date;

        // 1 = lunes ... 7 = domingo
        int diaSemana = fechaLocal.DayOfWeek == DayOfWeek.Sunday
            ? 7
            : (int)fechaLocal.DayOfWeek;

        if (!usuario.Horario.DiasLaborales.Contains(diaSemana))
            return (false, "El empleado no tiene jornada laboral programada para hoy.", null);

        // Fecha funciona como identificador del día laboral.
        // Se almacena normalizada a medianoche UTC.
        var fechaRegistro = DateTime.SpecifyKind(
            fechaLocal, DateTimeKind.Utc);

        var existente = await _repo.ObtenerPorUsuarioYFecha(
            usuarioId, fechaRegistro);

        if (existente != null)
            return (false, "Ya existe un registro de asistencia para hoy.", null);

        if (!TimeOnly.TryParse(
            usuario.Horario.HoraEntrada,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var horaEntradaProgramada))
        {
            return (false, "El horario de entrada del empleado no es válido.", null);
        }

        var horaProgramada = fechaLocal.Add(
            horaEntradaProgramada.ToTimeSpan());

        int minutosTardanza = Math.Max(
            0,
            (int)Math.Floor(
                (ahoraLocal - horaProgramada).TotalMinutes));

        var asistencia = new Asistencia
        {
            UsuarioId = usuarioId,
            Fecha = fechaRegistro,
            HoraEntrada = ahoraUtc,
            Estado = minutosTardanza > 0
                ? EstadosAsistencia.Tardanza
                : EstadosAsistencia.Pendiente,
            MinutosTardanza = minutosTardanza,
            FechaCreacion = ahoraUtc
        };

        await _repo.Crear(asistencia);

        return (true, null, asistencia);
    }

    // Registrar salida del empleado
    public async Task<(bool ok, string? error, Asistencia? asistencia)>
        RegistrarSalida(string asistenciaId)
    {
        var asistencia = await _repo.ObtenerPorId(asistenciaId);

        if (asistencia == null)
            return (false, "Registro de asistencia no encontrado.", null);

        if (asistencia.HoraEntrada == null)
            return (false, "No existe una marca de entrada.", null);

        if (asistencia.HoraSalida != null)
            return (false, "La salida ya fue registrada.", null);

        var usuario = await _usuarioRepo.ObtenerPorId(
            asistencia.UsuarioId);

        if (usuario == null)
            return (false, "No se encontró el empleado asociado.", null);

        var ahoraUtc = DateTime.UtcNow;
        var ahoraLocal = TimeZoneInfo.ConvertTimeFromUtc(
            ahoraUtc, ZonaCostaRica);

        if (!TimeOnly.TryParse(
            usuario.Horario.HoraSalida,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var horaSalidaProgramada))
        {
            return (false, "El horario de salida del empleado no es válido.", null);
        }

        var fechaLocal = DateTime.SpecifyKind(
            asistencia.Fecha.Date, DateTimeKind.Unspecified);

        var horaProgramada = fechaLocal.Add(
            horaSalidaProgramada.ToTimeSpan());

        int minutosSalidaAnticipada = Math.Max(
            0,
            (int)Math.Floor(
                (horaProgramada - ahoraLocal).TotalMinutes));

        asistencia.HoraSalida = ahoraUtc;
        asistencia.MinutosSalidaAnticipada =
            minutosSalidaAnticipada;

        if (minutosSalidaAnticipada > 0)
        {
            asistencia.Estado = EstadosAsistencia.SalidaAnticipada;
        }
        else if (asistencia.MinutosTardanza > 0)
        {
            asistencia.Estado = EstadosAsistencia.Tardanza;
        }
        else
        {
            asistencia.Estado = EstadosAsistencia.Completa;
        }

        await _repo.Actualizar(asistencia);

        return (true, null, asistencia);
    }

    // Consultar historial de un empleado
    public async Task<List<Asistencia>> ObtenerPorUsuario(
        string usuarioId)
    {
        return await _repo.ObtenerPorUsuario(usuarioId);
    }

    // Generar resumen mensual
    public async Task<(bool ok, string? error, AsistenciaResumenMensualDto? resumen)>
        ObtenerResumenMensual(string usuarioId, int anio, int mes)
    {
        if (mes < 1 || mes > 12)
            return (false, "El mes debe estar entre 1 y 12.", null);

        if (anio < 2000 || anio > 9999)
            return (false, "El año indicado no es válido.", null);

        var usuario = await _usuarioRepo.ObtenerPorId(usuarioId);

        if (usuario == null)
            return (false, "Empleado no encontrado.", null);

        var fechaInicio = new DateTime(
            anio, mes, 1, 0, 0, 0, DateTimeKind.Utc);

        var fechaFin = fechaInicio.AddMonths(1);

        var asistencias = await _repo.ObtenerPorPeriodo(
            usuarioId, fechaInicio, fechaFin);

        var resumen = new AsistenciaResumenMensualDto
        {
            UsuarioId = usuarioId,
            Anio = anio,
            Mes = mes,
            DiasRegistrados = asistencias.Count,
            DiasCompletos = asistencias.Count(a =>
                a.Estado == EstadosAsistencia.Completa),
            Tardanzas = asistencias.Count(a =>
                a.MinutosTardanza > 0),
            Ausencias = asistencias.Count(a =>
                a.Estado == EstadosAsistencia.Ausencia),
            SalidasAnticipadas = asistencias.Count(a =>
                a.MinutosSalidaAnticipada > 0),
            TotalMinutosTardanza = asistencias.Sum(a =>
                a.MinutosTardanza)
        };

        return (true, null, resumen);
    }

    // Obtener zona horaria de Costa Rica
    private static TimeZoneInfo ObtenerZonaCostaRica()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(
                "America/Costa_Rica");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById(
                "Central America Standard Time");
        }
    }
}
