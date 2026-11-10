using Nomina_back_end.DTOs;
using Nomina_back_end.Entities;
using Nomina_back_end.Repositories;
using System.Globalization;

namespace Nomina_back_end.Services;

public class AsistenciaService
{
    private readonly AsistenciaRepository _repo;
    private readonly UsuarioRepository _usuarios;

    public AsistenciaService(AsistenciaRepository repo, UsuarioRepository usuarios)
    {
        _repo = repo;
        _usuarios = usuarios;
    }

    // ---------- Marcas ----------

    public async Task<(Asistencia? asistencia, string? error)> MarcarEntrada(string usuarioId)
    {
        var u = await _usuarios.ObtenerPorId(usuarioId);
        if (u == null || u.Estado != EstadosUsuario.Activo)
            return (null, "Usuario no válido.");

        var ahora = DateTime.UtcNow;
        var fecha = HoraCR.Fecha(ahora);

        if (await _repo.ObtenerPorUsuarioYFecha(usuarioId, fecha) != null)
            return (null, "Ya existe un registro de asistencia para hoy.");

        var a = new Asistencia { UsuarioId = usuarioId, Fecha = fecha, HoraEntrada = ahora };
        Calcular(a, u.Horario);

        await _repo.Crear(a);
        return (a, null);
    }

    public async Task<(Asistencia? asistencia, string? error)> MarcarSalida(string usuarioId)
    {
        var u = await _usuarios.ObtenerPorId(usuarioId);
        if (u == null || u.Estado != EstadosUsuario.Activo)
            return (null, "Usuario no válido.");

        var ahora = DateTime.UtcNow;
        var a = await _repo.ObtenerPorUsuarioYFecha(usuarioId, HoraCR.Fecha(ahora));

        if (a == null || a.HoraEntrada == null)
            return (null, "Primero debes marcar la entrada.");
        if (a.HoraSalida != null)
            return (null, "Ya registraste la salida de hoy.");

        a.HoraSalida = ahora;
        Calcular(a, u.Horario);

        await _repo.Reemplazar(a);
        return (a, null);
    }

    // ---------- Consultas ----------

    public async Task<List<(Asistencia asistencia, string nombre)>> Historial(
        string usuarioId, string? desde, string? hasta)
    {
        var u = await _usuarios.ObtenerPorId(usuarioId);
        if (u == null) return new();

        var registros = await _repo.Buscar(new[] { usuarioId }, desde, hasta);
        return registros.Select(r => (r, u.Nombre)).ToList();
    }

    public async Task<List<(Asistencia asistencia, string nombre)>> Listar(
        string solicitanteId, string rol,
        string? usuarioId, string? departamentoId, string? desde, string? hasta)
    {
        // RR.HH. ve a todos; Jefatura solo a su equipo
        IEnumerable<Usuario> universo = rol == Roles.RRHH
            ? await _usuarios.ObtenerTodos()
            : await _usuarios.ObtenerPorJefatura(solicitanteId);

        if (!string.IsNullOrWhiteSpace(usuarioId))
            universo = universo.Where(u => u.Id == usuarioId);
        if (!string.IsNullOrWhiteSpace(departamentoId))
            universo = universo.Where(u => u.DepartamentoId == departamentoId);

        var lista = universo.ToList();
        if (lista.Count == 0) return new();

        var registros = await _repo.Buscar(lista.Select(u => u.Id!), desde, hasta);
        var nombres = lista.ToDictionary(u => u.Id!, u => u.Nombre);

        return registros.Select(r => (r, nombres[r.UsuarioId])).ToList();
    }

    public async Task<bool> PuedeConsultar(string solicitanteId, string rol, string objetivoId)
    {
        if (rol == Roles.RRHH || solicitanteId == objetivoId) return true;

        if (rol == Roles.Jefatura)
        {
            var objetivo = await _usuarios.ObtenerPorId(objetivoId);
            return objetivo?.JefaturaDirectaId == solicitanteId;
        }

        return false;
    }

    // ---------- Ajuste manual (RR.HH.) ----------

    public async Task<(bool ok, string? error)> Ajustar(
        string ajustadoPorId, string usuarioId, string fecha,
        string? horaEntrada, string? horaSalida, string justificacion)
    {
        var u = await _usuarios.ObtenerPorId(usuarioId);
        if (u == null) return (false, "Empleado no encontrado.");

        if (!DateTime.TryParseExact(fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out _))
            return (false, "Fecha inválida.");

        if (horaEntrada == null && horaSalida == null)
            return (false, "Indica al menos la hora de entrada o la de salida.");

        var a = await _repo.ObtenerPorUsuarioYFecha(usuarioId, fecha);
        var esNuevo = a == null;
        a ??= new Asistencia { UsuarioId = usuarioId, Fecha = fecha };

        var ajuste = new AjusteAsistencia
        {
            AjustadoPorId = ajustadoPorId,
            Justificacion = justificacion.Trim(),
            EntradaAnterior = a.HoraEntrada,
            SalidaAnterior = a.HoraSalida
        };

        if (horaEntrada != null) a.HoraEntrada = HoraCR.AUtc(fecha, horaEntrada);
        if (horaSalida != null) a.HoraSalida = HoraCR.AUtc(fecha, horaSalida);

        if (a.HoraEntrada != null && a.HoraSalida != null && a.HoraSalida <= a.HoraEntrada)
            return (false, "La salida debe ser posterior a la entrada.");

        a.Ajustes.Add(ajuste);
        Calcular(a, u.Horario);

        if (esNuevo) await _repo.Crear(a);
        else await _repo.Reemplazar(a);

        return (true, null);
    }

    // ---------- Resumen mensual ----------

    public async Task<ResumenAsistenciaDto?> Resumen(string usuarioId, int anio, int mes)
    {
        var u = await _usuarios.ObtenerPorId(usuarioId);
        if (u == null) return null;

        var primero = new DateTime(anio, mes, 1);
        var ultimo = primero.AddMonths(1).AddDays(-1);

        var registros = await _repo.Buscar(new[] { usuarioId },
            primero.ToString("yyyy-MM-dd"), ultimo.ToString("yyyy-MM-dd"));

        var conEntrada = registros.Where(r => r.HoraEntrada != null).ToList();
        var fechasConMarca = conEntrada.Select(r => r.Fecha).ToHashSet();

        // Ausencia: día laboral del horario, ya pasado, sin marca de entrada
        var hoy = HoraCR.ALocal(DateTime.UtcNow).Date;
        var inicio = u.FechaIngreso.Date > primero ? u.FechaIngreso.Date : primero;
        var ausencias = new List<string>();

        for (var d = inicio; d <= ultimo && d < hoy; d = d.AddDays(1))
        {
            var texto = d.ToString("yyyy-MM-dd");
            if (u.Horario.DiasLaborales.Contains(HoraCR.DiaSemana(d)) && !fechasConMarca.Contains(texto))
                ausencias.Add(texto);
        }

        return new ResumenAsistenciaDto
        {
            UsuarioId = usuarioId,
            NombreEmpleado = u.Nombre,
            Anio = anio,
            Mes = mes,
            DiasLaborados = conEntrada.Count,
            Tardias = conEntrada.Count(r => r.Tardia),
            SalidasAnticipadas = registros.Count(r => r.SalidaAnticipada),
            HorasTrabajadas = registros.Sum(r => r.HorasTrabajadas),
            // Por ahora todas son injustificadas: cuando existan permisos,
            // vacaciones y feriados se separan aquí.
            AusenciasJustificadas = 0,
            AusenciasInjustificadas = ausencias.Count,
            FechasAusencia = ausencias
        };
    }

    // ---------- Cálculo de tardías, salidas anticipadas y horas ----------

    private static void Calcular(Asistencia a, Horario horario)
    {
        a.Tardia = false;
        a.MinutosTardia = 0;
        a.SalidaAnticipada = false;
        a.MinutosSalidaAnticipada = 0;
        a.HorasTrabajadas = 0;

        if (a.HoraEntrada != null)
        {
            var entradaLocal = HoraCR.ALocal(a.HoraEntrada.Value).TimeOfDay;
            var esperada = TimeSpan.Parse(horario.HoraEntrada);
            var minutos = (int)Math.Floor((entradaLocal - esperada).TotalMinutes);

            if (minutos > 0)
            {
                a.Tardia = true;
                a.MinutosTardia = minutos;
            }
        }

        if (a.HoraSalida != null)
        {
            var salidaLocal = HoraCR.ALocal(a.HoraSalida.Value).TimeOfDay;
            var esperada = TimeSpan.Parse(horario.HoraSalida);
            var minutos = (int)Math.Floor((esperada - salidaLocal).TotalMinutes);

            if (minutos > 0)
            {
                a.SalidaAnticipada = true;
                a.MinutosSalidaAnticipada = minutos;
            }
        }

        if (a.HoraEntrada != null && a.HoraSalida != null && a.HoraSalida > a.HoraEntrada)
            a.HorasTrabajadas = Math.Round((decimal)(a.HoraSalida.Value - a.HoraEntrada.Value).TotalHours, 2);
    }
}