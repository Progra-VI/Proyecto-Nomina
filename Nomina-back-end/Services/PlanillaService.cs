using System.Globalization;
using Nomina_back_end.DTOs;
using Nomina_back_end.Entities;
using Nomina_back_end.Repositories;

namespace Nomina_back_end.Services;

public class PlanillaService
{
    private const string ClaveDiasMes = "planilla.dias_por_mes";
    private const string ClavePeriodos = "planilla.periodos_por_mes";

    private readonly PlanillaRepository _repo;
    private readonly BoletaRepository _boletas;
    private readonly ConfiguracionLeyRepository _ley;
    private readonly UsuarioRepository _usuarios;
    private readonly DepartamentoRepository _departamentos;
    private readonly AsistenciaRepository _asistencias;
    private readonly PermisoRepository _permisos;
    private readonly VacacionRepository _vacaciones;
    private readonly HoraExtraRepository _horasExtra;
    private readonly FeriadoRepository _feriados;
    private readonly ParametroService _parametros;

    public PlanillaService(PlanillaRepository repo, BoletaRepository boletas, ConfiguracionLeyRepository ley,
        UsuarioRepository usuarios, DepartamentoRepository departamentos, AsistenciaRepository asistencias,
        PermisoRepository permisos, VacacionRepository vacaciones, HoraExtraRepository horasExtra,
        FeriadoRepository feriados, ParametroService parametros)
    {
        _repo = repo;
        _boletas = boletas;
        _ley = ley;
        _usuarios = usuarios;
        _departamentos = departamentos;
        _asistencias = asistencias;
        _permisos = permisos;
        _vacaciones = vacaciones;
        _horasExtra = horasExtra;
        _feriados = feriados;
        _parametros = parametros;
    }

    private record Contexto(decimal DiasMes, decimal Periodos, ConfiguracionLey Ley,
        HashSet<string> Feriados, DateTime Hoy);

    // ---------- Crear ----------

    public async Task<(Resultado r, PlanillaRespuestaDto? p)> Crear(string usuarioId, PlanillaCrearDto dto)
    {
        var periodos = await _parametros.ObtenerValor(ClavePeriodos);
        if (periodos != 1 && periodos != 2)
            return (Resultado.Fallo($"RR.HH. debe configurar el parámetro '{ClavePeriodos}' (1 = mensual, 2 = quincenal)."), null);

        var quincenal = periodos == 2;
        if (quincenal && dto.Quincena == null)
            return (Resultado.Fallo("La planilla es quincenal: indica la quincena (1 o 2)."), null);
        if (!quincenal && dto.Quincena != null)
            return (Resultado.Fallo("La planilla es mensual: no indiques quincena."), null);

        if (await _repo.ObtenerPorPeriodo(dto.Anio, dto.Mes, dto.Quincena) != null)
            return (Resultado.Fallo("Ya existe una planilla para ese periodo.", 409), null);

        var (desde, hasta) = Rango(dto.Anio, dto.Mes, dto.Quincena);
        var ahora = DateTime.UtcNow;

        var p = new Planilla
        {
            Anio = dto.Anio,
            Mes = dto.Mes,
            Quincena = dto.Quincena,
            Desde = desde,
            Hasta = hasta,
            FechaCreacion = ahora,
            Historial =
            {
                new CambioEstado { Estado = EstadosPlanilla.Borrador, UsuarioId = usuarioId, Fecha = ahora }
            }
        };

        await _repo.Crear(p);
        return (Resultado.Exito(), ADto(p));
    }

    // ---------- Calcular ----------

    public async Task<(Resultado r, PlanillaRespuestaDto? p)> Calcular(string id, string usuarioId)
    {
        var p = await _repo.ObtenerPorId(id);
        if (p == null) return (Resultado.Fallo("Planilla no encontrada.", 404), null);

        if (p.Estado != EstadosPlanilla.Borrador && p.Estado != EstadosPlanilla.Calculada)
            return (Resultado.Fallo("Una planilla aprobada o pagada no puede recalcularse.", 409), null);

        var diasMes = await _parametros.ObtenerValor(ClaveDiasMes);
        var periodos = await _parametros.ObtenerValor(ClavePeriodos);

        if (diasMes is null or <= 0 || (periodos != 1 && periodos != 2))
            return (Resultado.Fallo(
                $"RR.HH. debe configurar los parámetros '{ClaveDiasMes}' y '{ClavePeriodos}'."), null);

        var ley = await _ley.Obtener();
        if (ley == null)
            return (Resultado.Fallo("RR.HH. debe configurar los parámetros de ley (cargas sociales y tramos de renta)."), null);

        var hastaFecha = ParseFecha(p.Hasta);
        var empleados = (await _usuarios.ObtenerTodos())
            .Where(u => u.Estado == EstadosUsuario.Activo && u.FechaIngreso.Date <= hastaFecha)
            .ToList();

        if (empleados.Count == 0)
            return (Resultado.Fallo("No hay empleados activos para ese periodo."), null);

        var ids = empleados.Select(u => u.Id!).ToList();
        var desdeUtc = HoraCR.AUtc(p.Desde, "00:00");
        var hastaUtc = HoraCR.AUtc(p.Hasta, "23:59");

        var asistencias = (await _asistencias.Buscar(ids, p.Desde, p.Hasta))
            .GroupBy(a => a.UsuarioId).ToDictionary(g => g.Key, g => g.ToList());
        var permisos = (await _permisos.Buscar(ids, EstadosSolicitud.Aprobada, null, desdeUtc, hastaUtc))
            .GroupBy(x => x.UsuarioId).ToDictionary(g => g.Key, g => g.ToList());
        var vacaciones = (await _vacaciones.Buscar(ids, EstadosSolicitud.Aprobada, desdeUtc, hastaUtc))
            .GroupBy(x => x.UsuarioId).ToDictionary(g => g.Key, g => g.ToList());
        var extras = (await _horasExtra.Buscar(ids, EstadosSolicitud.Aprobada, null, p.Desde, p.Hasta))
            .GroupBy(x => x.UsuarioId).ToDictionary(g => g.Key, g => g.ToList());

        var feriados = (await _feriados.ObtenerPorAnio(p.Anio)).Select(f => f.Fecha).ToHashSet();
        var departamentos = (await _departamentos.ObtenerTodos()).ToDictionary(d => d.Id!, d => d.Nombre);

        var ctx = new Contexto(diasMes.Value, periodos!.Value, ley, feriados, HoraCR.ALocal(DateTime.UtcNow).Date);

        var boletas = empleados.Select(u =>
        {
            var b = CalcularBoleta(p, u,
                departamentos.GetValueOrDefault(u.DepartamentoId, "Sin departamento"), ctx,
                asistencias.GetValueOrDefault(u.Id!) ?? new(),
                permisos.GetValueOrDefault(u.Id!) ?? new(),
                vacaciones.GetValueOrDefault(u.Id!) ?? new(),
                extras.GetValueOrDefault(u.Id!) ?? new());
            b.PlanillaId = p.Id!;
            return b;
        }).ToList();

        var ahora = DateTime.UtcNow;
        p.Empleados = boletas.Count;
        p.TotalIngresos = boletas.Sum(b => b.TotalIngresos);
        p.TotalDeducciones = boletas.Sum(b => b.TotalDeducciones);
        p.TotalNeto = boletas.Sum(b => b.SalarioNeto);
        p.FechaCalculo = ahora;

        var cambio = new CambioEstado
        {
            Estado = EstadosPlanilla.Calculada,
            UsuarioId = usuarioId,
            Fecha = ahora,
            Observacion = "Cálculo de planilla"
        };

        if (!await _repo.GuardarCalculo(p, cambio))
            return (Resultado.Fallo("La planilla cambió de estado y ya no puede recalcularse.", 409), null);

        await _boletas.ReemplazarDePlanilla(p.Id!, boletas);

        p.Estado = EstadosPlanilla.Calculada;
        return (Resultado.Exito(), ADto(p));
    }

    private static BoletaPago CalcularBoleta(Planilla p, Usuario u, string departamento, Contexto c,
        List<Asistencia> asistencias, List<SolicitudPermiso> permisos,
        List<SolicitudVacaciones> vacaciones, List<SolicitudHoraExtra> extras)
    {
        var desde = ParseFecha(p.Desde);
        var hasta = ParseFecha(p.Hasta);
        var inicio = u.FechaIngreso.Date > desde ? u.FechaIngreso.Date : desde;

        var diasCalendarioPeriodo = (hasta - desde).Days + 1;
        var diasCalendarioContratado = (hasta - inicio).Days + 1;

        // Días del periodo en base de "mes comercial": 30 (mensual) o 15 (quincenal)
        var diasBase = c.DiasMes / c.Periodos;
        var factor = diasBase / diasCalendarioPeriodo;
        var salarioDiario = u.SalarioBase / c.DiasMes;

        var marcas = asistencias.Where(a => a.HoraEntrada != null).Select(a => a.Fecha).ToHashSet();

        var diasVacaciones = 0;
        var ausInjustificadas = 0;
        var ausJustificadas = 0;
        decimal deduccionPermisos = 0;

        for (var d = inicio; d <= hasta; d = d.AddDays(1))
        {
            var texto = d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var ini = HoraCR.AUtc(texto, "00:00");
            var fin = ini.AddDays(1);

            // Vacaciones: día pagado, sin deducciones
            if (vacaciones.Any(v => v.FechaInicio < fin && v.FechaFin > ini))
            {
                diasVacaciones++;
                continue;
            }

            var laboral = u.Horario.DiasLaborales.Contains(HoraCR.DiaSemana(d)) && !c.Feriados.Contains(texto);
            if (!laboral) continue;

            var permisosDia = permisos.Where(x => x.FechaInicio < fin && x.FechaFin > ini).ToList();

            // Permisos sin goce: se descuenta la fracción de jornada cubierta
            var entrada = HoraCR.AUtc(texto, u.Horario.HoraEntrada);
            var salida = HoraCR.AUtc(texto, u.Horario.HoraSalida);
            var jornada = (salida - entrada).TotalHours;

            if (jornada > 0)
            {
                var horasSinGoce = permisosDia.Where(x => !x.ConGoce)
                    .Sum(x => Solape(x.FechaInicio, x.FechaFin, entrada, salida));

                if (horasSinGoce > 0)
                    deduccionPermisos += Math.Min(1m, (decimal)(horasSinGoce / jornada)) * salarioDiario;
            }

            // Ausencias: solo días ya pasados y sin marca de entrada
            if (d < c.Hoy && !marcas.Contains(texto))
            {
                if (permisosDia.Count > 0) ausJustificadas++;
                else ausInjustificadas++;
            }
        }

        var diasLaborados = (diasCalendarioContratado - diasVacaciones) * factor;

        var ingresoSalario = R(salarioDiario * diasLaborados);
        var ingresoVacaciones = R(salarioDiario * diasVacaciones * factor);
        var ingresoExtras = R(extras.Sum(x => x.Monto));
        var totalIngresos = ingresoSalario + ingresoVacaciones + ingresoExtras;

        var dedAusencias = R(ausInjustificadas * salarioDiario);
        var dedPermisos = R(deduccionPermisos);

        // Base de cargas sociales y renta: lo realmente devengado
        var baseImponible = Math.Max(0, totalIngresos - dedAusencias - dedPermisos);

        var cargas = c.Ley.CargasSociales.Select(x => new LineaCarga
        {
            Nombre = x.Nombre,
            Porcentaje = x.Porcentaje,
            Monto = R(baseImponible * x.Porcentaje / 100m)
        }).ToList();
        var totalCargas = cargas.Sum(x => x.Monto);

        // Los tramos son mensuales: se mensualiza la base y se reparte el impuesto entre los periodos
        var renta = R(CalcularRenta(baseImponible * c.Periodos, c.Ley.TramosRenta) / c.Periodos);

        var totalDeducciones = dedAusencias + dedPermisos + totalCargas + renta;

        return new BoletaPago
        {
            UsuarioId = u.Id!,
            NombreEmpleado = u.Nombre,
            Identificacion = u.Identificacion,
            DepartamentoId = u.DepartamentoId,
            Departamento = departamento,
            Puesto = u.Puesto,
            SalarioBase = u.SalarioBase,
            SalarioDiario = R(salarioDiario),
            DiasLaborados = R(diasLaborados),
            DiasVacaciones = R(diasVacaciones * factor),
            AusenciasInjustificadas = ausInjustificadas,
            AusenciasJustificadas = ausJustificadas,
            HorasExtra = extras.Sum(x => x.Horas),
            IngresoSalario = ingresoSalario,
            IngresoVacaciones = ingresoVacaciones,
            IngresoHorasExtra = ingresoExtras,
            TotalIngresos = totalIngresos,
            DeduccionAusencias = dedAusencias,
            DeduccionPermisos = dedPermisos,
            CargasSociales = cargas,
            TotalCargas = totalCargas,
            ImpuestoRenta = renta,
            TotalDeducciones = totalDeducciones,
            SalarioNeto = totalIngresos - totalDeducciones
        };
    }

    private static decimal CalcularRenta(decimal baseMensual, List<TramoRenta> tramos)
    {
        decimal total = 0;

        foreach (var t in tramos.OrderBy(x => x.Desde))
        {
            if (baseMensual <= t.Desde) break;

            var tope = t.Hasta ?? decimal.MaxValue;
            var porcion = Math.Min(baseMensual, tope) - t.Desde;
            total += porcion * t.Porcentaje / 100m;
        }

        return total;
    }

    private static double Solape(DateTime a1, DateTime a2, DateTime b1, DateTime b2)
    {
        var ini = a1 > b1 ? a1 : b1;
        var fin = a2 < b2 ? a2 : b2;
        return Math.Max(0, (fin - ini).TotalHours);
    }

    private static decimal R(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

    // ---------- Aprobar / pagar ----------

    public Task<Resultado> Aprobar(string id, string usuarioId) =>
        Transicion(id, usuarioId, EstadosPlanilla.Calculada, EstadosPlanilla.Aprobada);

    public Task<Resultado> Pagar(string id, string usuarioId) =>
        Transicion(id, usuarioId, EstadosPlanilla.Aprobada, EstadosPlanilla.Pagada);

    private async Task<Resultado> Transicion(string id, string usuarioId, string desde, string hasta)
    {
        var p = await _repo.ObtenerPorId(id);
        if (p == null) return Resultado.Fallo("Planilla no encontrada.", 404);

        if (p.Estado != desde)
            return Resultado.Fallo($"Solo una planilla en estado {desde} puede pasar a {hasta}.", 409);

        var cambio = new CambioEstado { Estado = hasta, UsuarioId = usuarioId };

        return await _repo.CambiarEstado(id, desde, hasta, cambio)
            ? Resultado.Exito()
            : Resultado.Fallo("La planilla ya cambió de estado.", 409);
    }

    // ---------- Consultas ----------

    public async Task<List<PlanillaRespuestaDto>> Listar(int? anio) =>
        (await _repo.Listar(anio)).Select(ADto).ToList();

    public async Task<(Resultado r, PlanillaDetalleDto? d)> ObtenerDetalle(string id)
    {
        var p = await _repo.ObtenerPorId(id);
        if (p == null) return (Resultado.Fallo("Planilla no encontrada.", 404), null);

        var boletas = await _boletas.ObtenerPorPlanilla(id);

        return (Resultado.Exito(), new PlanillaDetalleDto
        {
            Planilla = ADto(p),
            Boletas = boletas.Select(b => ADto(b, p)).ToList()
        });
    }

    public async Task<(Resultado r, ReportePlanillaDto? rep)> Reporte(string id)
    {
        var p = await _repo.ObtenerPorId(id);
        if (p == null) return (Resultado.Fallo("Planilla no encontrada.", 404), null);

        var boletas = await _boletas.ObtenerPorPlanilla(id);

        return (Resultado.Exito(), new ReportePlanillaDto
        {
            Planilla = ADto(p),
            PorDepartamento = boletas
                .GroupBy(b => b.DepartamentoId)
                .Select(g => new ReporteDepartamentoDto
                {
                    DepartamentoId = g.Key,
                    Departamento = g.First().Departamento,
                    Empleados = g.Count(),
                    TotalIngresos = g.Sum(b => b.TotalIngresos),
                    TotalDeducciones = g.Sum(b => b.TotalDeducciones),
                    TotalNeto = g.Sum(b => b.SalarioNeto)
                })
                .OrderBy(x => x.Departamento)
                .ToList()
        });
    }

    // El empleado solo ve boletas de planillas aprobadas o pagadas
    public async Task<List<BoletaDto>> BoletasMias(string usuarioId)
    {
        var boletas = await _boletas.ObtenerPorUsuario(usuarioId);
        var planillas = (await _repo.Listar(null)).ToDictionary(p => p.Id!);

        return boletas
            .Where(b => planillas.TryGetValue(b.PlanillaId, out var p) && Visible(p))
            .Select(b => ADto(b, planillas[b.PlanillaId]))
            .OrderByDescending(b => b.Desde)
            .ToList();
    }

    public async Task<(Resultado r, BoletaDto? b)> ObtenerBoleta(string id, string solicitanteId, string rol)
    {
        var b = await _boletas.ObtenerPorId(id);
        if (b == null) return (Resultado.Fallo("Boleta no encontrada.", 404), null);

        var p = await _repo.ObtenerPorId(b.PlanillaId);
        if (p == null) return (Resultado.Fallo("Planilla no encontrada.", 404), null);

        if (rol != Roles.RRHH && (b.UsuarioId != solicitanteId || !Visible(p)))
            return (Resultado.Fallo("No tienes acceso a esta boleta.", 403), null);

        return (Resultado.Exito(), ADto(b, p));
    }

    // ---------- Utilidades ----------

    private static bool Visible(Planilla p) =>
        p.Estado == EstadosPlanilla.Aprobada || p.Estado == EstadosPlanilla.Pagada;

    private static DateTime ParseFecha(string f) =>
        DateTime.ParseExact(f, "yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static (string desde, string hasta) Rango(int anio, int mes, int? quincena)
    {
        var ultimo = DateTime.DaysInMonth(anio, mes);
        var (d1, d2) = quincena switch { 1 => (1, 15), 2 => (16, ultimo), _ => (1, ultimo) };
        return ($"{anio:0000}-{mes:00}-{d1:00}", $"{anio:0000}-{mes:00}-{d2:00}");
    }

    private static PlanillaRespuestaDto ADto(Planilla p) => new()
    {
        Id = p.Id,
        Anio = p.Anio,
        Mes = p.Mes,
        Quincena = p.Quincena,
        Desde = p.Desde,
        Hasta = p.Hasta,
        Estado = p.Estado,
        Empleados = p.Empleados,
        TotalIngresos = p.TotalIngresos,
        TotalDeducciones = p.TotalDeducciones,
        TotalNeto = p.TotalNeto,
        FechaCalculo = p.FechaCalculo
    };

    private static BoletaDto ADto(BoletaPago b, Planilla p) => new()
    {
        Id = b.Id,
        PlanillaId = b.PlanillaId,
        EstadoPlanilla = p.Estado,
        Anio = p.Anio,
        Mes = p.Mes,
        Quincena = p.Quincena,
        Desde = p.Desde,
        Hasta = p.Hasta,
        UsuarioId = b.UsuarioId,
        NombreEmpleado = b.NombreEmpleado,
        Identificacion = b.Identificacion,
        Departamento = b.Departamento,
        Puesto = b.Puesto,
        SalarioBase = b.SalarioBase,
        DiasLaborados = b.DiasLaborados,
        DiasVacaciones = b.DiasVacaciones,
        AusenciasInjustificadas = b.AusenciasInjustificadas,
        AusenciasJustificadas = b.AusenciasJustificadas,
        HorasExtra = b.HorasExtra,
        IngresoSalario = b.IngresoSalario,
        IngresoVacaciones = b.IngresoVacaciones,
        IngresoHorasExtra = b.IngresoHorasExtra,
        TotalIngresos = b.TotalIngresos,
        DeduccionAusencias = b.DeduccionAusencias,
        DeduccionPermisos = b.DeduccionPermisos,
        CargasSociales = b.CargasSociales
            .Select(x => new LineaCargaDto { Nombre = x.Nombre, Porcentaje = x.Porcentaje, Monto = x.Monto }).ToList(),
        TotalCargas = b.TotalCargas,
        ImpuestoRenta = b.ImpuestoRenta,
        TotalDeducciones = b.TotalDeducciones,
        SalarioNeto = b.SalarioNeto
    };
}