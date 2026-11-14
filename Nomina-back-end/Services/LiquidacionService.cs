using System.Globalization;
using Nomina_back_end.DTOs;
using Nomina_back_end.Entities;
using Nomina_back_end.Repositories;

namespace Nomina_back_end.Services;

public class LiquidacionService
{
    private const string ClaveDiasMes = "planilla.dias_por_mes";
    private const string ClaveMesesAguinaldo = "liquidacion.aguinaldo_meses";
    private const string ClaveMaxAnios = "liquidacion.cesantia_max_anios";

    private readonly LiquidacionRepository _repo;
    private readonly UsuarioRepository _usuarios;
    private readonly DepartamentoRepository _departamentos;
    private readonly MotivoLiquidacionRepository _motivos;
    private readonly PlanillaRepository _planillas;
    private readonly ConfiguracionLeyRepository _ley;
    private readonly VacacionService _vacaciones;
    private readonly ParametroService _parametros;
    private readonly ComprobanteStorage _storage;

    public LiquidacionService(LiquidacionRepository repo, UsuarioRepository usuarios,
        DepartamentoRepository departamentos, MotivoLiquidacionRepository motivos, PlanillaRepository planillas,
        ConfiguracionLeyRepository ley, VacacionService vacaciones, ParametroService parametros,
        ComprobanteStorage storage)
    {
        _repo = repo;
        _usuarios = usuarios;
        _departamentos = departamentos;
        _motivos = motivos;
        _planillas = planillas;
        _ley = ley;
        _vacaciones = vacaciones;
        _parametros = parametros;
        _storage = storage;
    }

    // ---------- Crear / recalcular / eliminar (solo Borrador) ----------

    public async Task<(Resultado r, LiquidacionDto? l)> Crear(string solicitanteId, LiquidacionCrearDto dto)
    {
        var u = await _usuarios.ObtenerPorId(dto.UsuarioId);
        if (u == null) return (Resultado.Fallo("Empleado no encontrado.", 404), null);
        if (u.Estado != EstadosUsuario.Activo) return (Resultado.Fallo("El empleado ya está inactivo."), null);
        if (await _repo.ExistePorUsuario(dto.UsuarioId))
            return (Resultado.Fallo("Ya existe una liquidación para este empleado.", 409), null);

        var (r, l) = await Armar(u, dto.MotivoLiquidacionId, dto.FechaSalida);
        if (!r.Ok) return (r, null);

        var ahora = DateTime.UtcNow;
        l!.Estado = EstadosLiquidacion.Borrador;
        l.FechaCreacion = ahora;
        l.Historial.Add(new CambioEstado
        {
            Estado = EstadosLiquidacion.Borrador,
            UsuarioId = solicitanteId,
            Fecha = ahora
        });

        await _repo.Crear(l);
        return (Resultado.Exito(), ADto(l));
    }

    public async Task<(Resultado r, LiquidacionDto? l)> Recalcular(
        string id, string solicitanteId, LiquidacionGuardarDto dto)
    {
        var actual = await _repo.ObtenerPorId(id);
        if (actual == null) return (Resultado.Fallo("Liquidación no encontrada.", 404), null);
        if (actual.Estado != EstadosLiquidacion.Borrador)
            return (Resultado.Fallo("Una liquidación aprobada o pagada no puede modificarse.", 409), null);

        var u = await _usuarios.ObtenerPorId(actual.UsuarioId);
        if (u == null || u.Estado != EstadosUsuario.Activo)
            return (Resultado.Fallo("El empleado ya está inactivo.", 409), null);

        var (r, nueva) = await Armar(u, dto.MotivoLiquidacionId, dto.FechaSalida);
        if (!r.Ok) return (r, null);

        nueva!.Id = id;
        nueva.Estado = EstadosLiquidacion.Borrador;
        nueva.FechaCreacion = actual.FechaCreacion;
        nueva.Historial = actual.Historial;
        nueva.Historial.Add(new CambioEstado
        {
            Estado = EstadosLiquidacion.Borrador,
            UsuarioId = solicitanteId,
            Observacion = "Liquidación recalculada"
        });

        if (!await _repo.ReemplazarSiBorrador(nueva))
            return (Resultado.Fallo("La liquidación ya fue aprobada y no puede modificarse.", 409), null);

        return (Resultado.Exito(), ADto(nueva));
    }

    public async Task<Resultado> Eliminar(string id)
    {
        var l = await _repo.ObtenerPorId(id);
        if (l == null) return Resultado.Fallo("Liquidación no encontrada.", 404);
        if (l.Estado != EstadosLiquidacion.Borrador)
            return Resultado.Fallo("Solo se pueden eliminar liquidaciones en Borrador.", 409);

        return await _repo.EliminarSiBorrador(id)
            ? Resultado.Exito()
            : Resultado.Fallo("La liquidación ya fue aprobada.", 409);
    }

    // ---------- Aprobar (inactiva al empleado y guarda el PDF) / pagar ----------

    public async Task<Resultado> Aprobar(string id, string solicitanteId)
    {
        var l = await _repo.ObtenerPorId(id);
        if (l == null) return Resultado.Fallo("Liquidación no encontrada.", 404);
        if (l.Estado != EstadosLiquidacion.Borrador)
            return Resultado.Fallo("Solo una liquidación en Borrador puede aprobarse.", 409);

        var u = await _usuarios.ObtenerPorId(l.UsuarioId);
        if (u == null || u.Estado != EstadosUsuario.Activo)
            return Resultado.Fallo("El empleado ya está inactivo.", 409);

        // 1) Generar el comprobante con el estado ya como Aprobada y guardarlo en GridFS
        l.Estado = EstadosLiquidacion.Aprobada;

        MongoDB.Bson.ObjectId archivoId;
        try
        {
            var pdf = LiquidacionPdf.Generar(ADto(l));
            archivoId = await _storage.Guardar(NombreArchivo(l), pdf, l.Id!);
        }
        catch
        {
            return Resultado.Fallo("No se pudo generar o guardar el comprobante. La liquidación sigue en Borrador.", 500);
        }

        // 2) Cambio de estado atómico: si otro usuario la aprobó antes, se descarta el archivo
        var cambio = new CambioEstado { Estado = EstadosLiquidacion.Aprobada, UsuarioId = solicitanteId };

        if (!await _repo.CambiarEstado(id, EstadosLiquidacion.Borrador, EstadosLiquidacion.Aprobada,
                cambio, archivoId.ToString()))
        {
            await _storage.Eliminar(archivoId);
            return Resultado.Fallo("La liquidación ya fue aprobada por otro usuario.", 409);
        }

        // 3) El empleado deja de incluirse en la planilla (la planilla solo toma usuarios activos)
        await _usuarios.CambiarEstado(l.UsuarioId, EstadosUsuario.Inactivo);

        return Resultado.Exito();
    }

    public async Task<Resultado> Pagar(string id, string solicitanteId)
    {
        var l = await _repo.ObtenerPorId(id);
        if (l == null) return Resultado.Fallo("Liquidación no encontrada.", 404);
        if (l.Estado != EstadosLiquidacion.Aprobada)
            return Resultado.Fallo("Solo una liquidación Aprobada puede pasar a Pagada.", 409);

        var cambio = new CambioEstado { Estado = EstadosLiquidacion.Pagada, UsuarioId = solicitanteId };

        return await _repo.CambiarEstado(id, EstadosLiquidacion.Aprobada, EstadosLiquidacion.Pagada, cambio)
            ? Resultado.Exito()
            : Resultado.Fallo("La liquidación ya cambió de estado.", 409);
    }

    // ---------- Consultas ----------

    public async Task<List<LiquidacionDto>> Listar(string? estado, string? usuarioId) =>
        (await _repo.Listar(estado, usuarioId)).Select(ADto).ToList();

    public async Task<(Resultado r, LiquidacionDto? l)> ObtenerPorId(string id)
    {
        var l = await _repo.ObtenerPorId(id);
        return l == null
            ? (Resultado.Fallo("Liquidación no encontrada.", 404), null)
            : (Resultado.Exito(), ADto(l));
    }

    // Aprobada o Pagada: el PDF guardado en GridFS. Borrador: vista previa generada al momento
    public async Task<(Resultado r, byte[]? pdf, string? nombre)> ObtenerPdf(string id)
    {
        var l = await _repo.ObtenerPorId(id);
        if (l == null) return (Resultado.Fallo("Liquidación no encontrada.", 404), null, null);

        if (l.ComprobanteId != null)
        {
            var guardado = await _storage.Descargar(l.ComprobanteId);
            if (guardado != null) return (Resultado.Exito(), guardado, NombreArchivo(l));
        }

        return (Resultado.Exito(), LiquidacionPdf.Generar(ADto(l)), NombreArchivo(l));
    }

    // ---------- Cálculo ----------

    private async Task<(Resultado r, Liquidacion? l)> Armar(Usuario u, string motivoId, string fechaSalida)
    {
        var motivo = await _motivos.ObtenerPorId(motivoId);
        if (motivo == null) return (Resultado.Fallo("El motivo de liquidación no existe."), null);

        if (!DateTime.TryParseExact(fechaSalida, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var salida))
            return (Resultado.Fallo("Fecha de salida inválida."), null);

        var ingreso = u.FechaIngreso.Date;
        if (salida < ingreso)
            return (Resultado.Fallo("La fecha de salida no puede ser anterior a la fecha de ingreso."), null);

        var diasMes = await _parametros.ObtenerValor(ClaveDiasMes);
        var mesesAguinaldo = await _parametros.ObtenerValor(ClaveMesesAguinaldo);
        var maxAnios = await _parametros.ObtenerValor(ClaveMaxAnios);

        var faltan = new List<string>();
        if (diasMes is null or <= 0) faltan.Add(ClaveDiasMes);
        if (mesesAguinaldo is null or <= 0) faltan.Add(ClaveMesesAguinaldo);
        if (maxAnios is null or <= 0) faltan.Add(ClaveMaxAnios);

        if (faltan.Count > 0)
            return (Resultado.Fallo("RR.HH. debe configurar los parámetros: " + string.Join(", ", faltan) + "."), null);

        var ley = await _ley.Obtener();
        if (ley == null)
            return (Resultado.Fallo("RR.HH. debe configurar los parámetros de ley."), null);

        var salarioDiario = u.SalarioBase / diasMes!.Value;
        var meses = MesesCompletos(ingreso, salida);
        var antig = AntiguedadTexto(meses);

        var ingresos = new List<RubroLiquidacion>();
        var deducciones = new List<RubroLiquidacion>();

        // 1) Salario pendiente: desde el día siguiente a la última planilla aprobada hasta la salida
        var inicioPendiente = await InicioSalarioPendiente(salida, ingreso);
        var diasPendientes = Math.Max(0, (salida - inicioPendiente).Days + 1);
        var salarioPendiente = R(diasPendientes * salarioDiario);

        ingresos.Add(new RubroLiquidacion
        {
            Concepto = "Salario pendiente",
            Detalle = diasPendientes > 0
                ? $"{diasPendientes} día(s), del {inicioPendiente:yyyy-MM-dd} al {salida:yyyy-MM-dd}"
                : "Sin días pendientes de pago",
            Monto = salarioPendiente
        });

        // 2) Vacaciones proporcionales no disfrutadas (saldo a la fecha de salida)
        var (rv, saldoVacaciones) = await _vacaciones.SaldoAlDia(u.Id!, salida);
        if (!rv.Ok) return (rv, null);

        var montoVacaciones = R(saldoVacaciones * salarioDiario);
        ingresos.Add(new RubroLiquidacion
        {
            Concepto = "Vacaciones proporcionales",
            Detalle = $"{saldoVacaciones:0.##} día(s) de saldo no disfrutado",
            Monto = montoVacaciones
        });

        // 3) Aguinaldo proporcional: periodo del 1 de diciembre al 30 de noviembre
        var inicioPeriodo = salida.Month == 12
            ? new DateTime(salida.Year, 12, 1)
            : new DateTime(salida.Year - 1, 12, 1);
        var desdeAguinaldo = ingreso > inicioPeriodo ? ingreso : inicioPeriodo;
        var diasAguinaldo = (salida - desdeAguinaldo).Days + 1;
        var aguinaldo = R(salarioDiario * diasAguinaldo / mesesAguinaldo!.Value);

        ingresos.Add(new RubroLiquidacion
        {
            Concepto = "Aguinaldo proporcional",
            Detalle = $"{diasAguinaldo} día(s) del periodo {desdeAguinaldo:yyyy-MM-dd} al {salida:yyyy-MM-dd}, entre {mesesAguinaldo:0.##} meses",
            Monto = aguinaldo
        });

        // 4) Preaviso (solo si el motivo lo paga)
        if (motivo.PagaPreaviso)
        {
            var tramo = ley.PreavisoTramos.FirstOrDefault(t =>
                meses >= t.MesesDesde && (t.MesesHasta == null || meses < t.MesesHasta));

            var dias = tramo?.Dias ?? 0;
            ingresos.Add(new RubroLiquidacion
            {
                Concepto = "Preaviso",
                Detalle = tramo == null
                    ? $"Sin preaviso para una antigüedad de {antig}"
                    : $"{dias:0.##} día(s) de salario ({antig})",
                Monto = R(dias * salarioDiario)
            });
        }

        // 5) Auxilio de cesantía (solo si el motivo lo paga)
        if (motivo.PagaCesantia)
        {
            decimal dias;
            string detalle;

            if (meses < 12)
            {
                var tramo = ley.CesantiaMenorUnAnio.FirstOrDefault(t =>
                    meses >= t.MesesDesde && (t.MesesHasta == null || meses < t.MesesHasta));

                dias = tramo?.Dias ?? 0;
                detalle = tramo == null
                    ? $"Sin derecho a cesantía con {antig}"
                    : $"{dias:0.##} día(s) de salario ({antig})";
            }
            else
            {
                // Años completos más uno si la fracción pasa de seis meses, con tope configurable
                var anios = Math.Min((int)maxAnios!.Value, meses / 12 + (meses % 12 > 6 ? 1 : 0));
                var tarifa = ley.CesantiaPorAnio.FirstOrDefault(x => x.Anios == anios);

                if (tarifa == null)
                    return (Resultado.Fallo(
                        $"Falta el año {anios} en la tabla de cesantía (configuración de ley)."), null);

                dias = tarifa.DiasPorAnio * anios;
                detalle = $"{anios} año(s) reconocido(s) x {tarifa.DiasPorAnio:0.##} días = {dias:0.##} día(s) de salario";
            }

            ingresos.Add(new RubroLiquidacion
            {
                Concepto = "Auxilio de cesantía",
                Detalle = detalle,
                Monto = R(dias * salarioDiario)
            });
        }

        // Deducciones: cargas sociales y renta sobre el salario (pendiente + vacaciones).
        // Aguinaldo, preaviso y cesantía no llevan deducciones.
        var baseSalarial = salarioPendiente + montoVacaciones;

        foreach (var carga in ley.CargasSociales)
        {
            deducciones.Add(new RubroLiquidacion
            {
                Concepto = carga.Nombre,
                Detalle = $"{carga.Porcentaje:0.##}% sobre {baseSalarial:N2}",
                Monto = R(baseSalarial * carga.Porcentaje / 100m)
            });
        }

        deducciones.Add(new RubroLiquidacion
        {
            Concepto = "Impuesto sobre la renta",
            Detalle = "Sobre salario pendiente y vacaciones",
            Monto = R(PlanillaService.CalcularRenta(baseSalarial, ley.TramosRenta))
        });

        var departamento = await _departamentos.ObtenerPorId(u.DepartamentoId);
        var totalIngresos = ingresos.Sum(x => x.Monto);
        var totalDeducciones = deducciones.Sum(x => x.Monto);

        return (Resultado.Exito(), new Liquidacion
        {
            UsuarioId = u.Id!,
            MotivoId = motivo.Id!,
            Motivo = motivo.Nombre,
            NombreEmpleado = u.Nombre,
            Identificacion = u.Identificacion,
            Departamento = departamento?.Nombre ?? "Sin departamento",
            Puesto = u.Puesto,
            FechaIngreso = ingreso.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            FechaSalida = fechaSalida,
            AntiguedadMeses = meses,
            SalarioBase = u.SalarioBase,
            Ingresos = ingresos,
            Deducciones = deducciones,
            TotalIngresos = totalIngresos,
            TotalDeducciones = totalDeducciones,
            TotalNeto = totalIngresos - totalDeducciones
        });
    }

    private async Task<DateTime> InicioSalarioPendiente(DateTime salida, DateTime ingreso)
    {
        var textoSalida = salida.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        var cerradas = (await _planillas.Listar(null))
            .Where(p => (p.Estado == EstadosPlanilla.Aprobada || p.Estado == EstadosPlanilla.Pagada)
                        && string.CompareOrdinal(p.Desde, textoSalida) <= 0)
            .ToList();

        // Sin planillas cerradas: se paga desde el primer día del mes de salida
        var inicio = cerradas.Count > 0
            ? DateTime.ParseExact(cerradas.Max(p => p.Hasta), "yyyy-MM-dd", CultureInfo.InvariantCulture).AddDays(1)
            : new DateTime(salida.Year, salida.Month, 1);

        return inicio > ingreso ? inicio : ingreso;
    }

    private static int MesesCompletos(DateTime ingreso, DateTime salida)
    {
        var meses = (salida.Year - ingreso.Year) * 12 + salida.Month - ingreso.Month;
        if (salida.Day < ingreso.Day) meses--;
        return Math.Max(0, meses);
    }

    private static string AntiguedadTexto(int meses) => $"{meses / 12} año(s) y {meses % 12} mes(es)";

    private static decimal R(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

    private static string NombreArchivo(Liquidacion l) => $"liquidacion-{l.Identificacion}-{l.FechaSalida}.pdf";

    private static LiquidacionDto ADto(Liquidacion l) => new()
    {
        Id = l.Id,
        UsuarioId = l.UsuarioId,
        NombreEmpleado = l.NombreEmpleado,
        Identificacion = l.Identificacion,
        Departamento = l.Departamento,
        Puesto = l.Puesto,
        FechaIngreso = l.FechaIngreso,
        FechaSalida = l.FechaSalida,
        MotivoId = l.MotivoId,
        Motivo = l.Motivo,
        AntiguedadMeses = l.AntiguedadMeses,
        Antiguedad = AntiguedadTexto(l.AntiguedadMeses),
        SalarioBase = l.SalarioBase,
        Estado = l.Estado,
        Ingresos = l.Ingresos.Select(x => new RubroDto { Concepto = x.Concepto, Detalle = x.Detalle, Monto = x.Monto }).ToList(),
        Deducciones = l.Deducciones.Select(x => new RubroDto { Concepto = x.Concepto, Detalle = x.Detalle, Monto = x.Monto }).ToList(),
        TotalIngresos = l.TotalIngresos,
        TotalDeducciones = l.TotalDeducciones,
        TotalNeto = l.TotalNeto,
        TieneComprobante = l.ComprobanteId != null
    };
}