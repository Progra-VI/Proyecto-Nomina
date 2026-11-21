using Nomina_back_end.DTOs;
using Nomina_back_end.Entities;
using Nomina_back_end.Repositories;

namespace Nomina_back_end.Services;

public class ConfiguracionLeyService
{
    private readonly ConfiguracionLeyRepository _repo;

    public ConfiguracionLeyService(ConfiguracionLeyRepository repo)
    {
        _repo = repo;
    }

    public async Task<ConfiguracionLeyDto> Obtener()
    {
        var c = await _repo.Obtener();
        return new ConfiguracionLeyDto
        {
            CargasSociales = c?.CargasSociales
                .Select(x => new CargaSocialDto { Nombre = x.Nombre, Porcentaje = x.Porcentaje }).ToList() ?? new(),
            TramosRenta = c?.TramosRenta
                .Select(x => new TramoRentaDto { Desde = x.Desde, Hasta = x.Hasta, Porcentaje = x.Porcentaje }).ToList() ?? new(),
            PreavisoTramos = c?.PreavisoTramos
                .Select(x => new TramoDiasDto { MesesDesde = x.MesesDesde, MesesHasta = x.MesesHasta, Dias = x.Dias }).ToList() ?? new(),
            CesantiaMenorUnAnio = c?.CesantiaMenorUnAnio
                .Select(x => new TramoDiasDto { MesesDesde = x.MesesDesde, MesesHasta = x.MesesHasta, Dias = x.Dias }).ToList() ?? new(),
            CesantiaPorAnio = c?.CesantiaPorAnio.OrderBy(x => x.Anios)
                .Select(x => new DiasCesantiaAnioDto { Anios = x.Anios, DiasPorAnio = x.DiasPorAnio }).ToList() ?? new()
        };
    }

    public async Task<Resultado> Guardar(string usuarioId, ConfiguracionLeyDto dto)
    {
        var nombres = dto.CargasSociales.Select(c => c.Nombre.Trim().ToLower()).ToList();
        if (nombres.Distinct().Count() != nombres.Count)
            return Resultado.Fallo("Hay cargas sociales con el mismo nombre.");

        var tramos = dto.TramosRenta.OrderBy(t => t.Desde).ToList();

        if (tramos.Count > 0)
        {
            if (tramos[0].Desde != 0)
                return Resultado.Fallo("El primer tramo de renta debe iniciar en 0.");

            for (var i = 0; i < tramos.Count; i++)
            {
                var esUltimo = i == tramos.Count - 1;

                if (esUltimo)
                {
                    if (tramos[i].Hasta != null)
                        return Resultado.Fallo("El último tramo no debe tener límite superior (hasta vacío).");
                }
                else
                {
                    if (tramos[i].Hasta == null || tramos[i].Hasta <= tramos[i].Desde)
                        return Resultado.Fallo("Cada tramo (menos el último) necesita un 'hasta' mayor que su 'desde'.");

                    if (tramos[i + 1].Desde != tramos[i].Hasta)
                        return Resultado.Fallo("Los tramos deben ser continuos: el 'desde' de uno es el 'hasta' del anterior.");
                }
            }
        }

        // NUEVO: preaviso y cesantía
        var preaviso = dto.PreavisoTramos.OrderBy(t => t.MesesDesde).ToList();
        var cesantiaMenor = dto.CesantiaMenorUnAnio.OrderBy(t => t.MesesDesde).ToList();

        var errorTramos = ValidarTramosMeses(preaviso, "preaviso") ?? ValidarTramosMeses(cesantiaMenor, "cesantía");
        if (errorTramos != null) return Resultado.Fallo(errorTramos);

        if (dto.CesantiaPorAnio.Select(x => x.Anios).Distinct().Count() != dto.CesantiaPorAnio.Count)
            return Resultado.Fallo("Hay años repetidos en la tabla de cesantía.");

        await _repo.Guardar(new ConfiguracionLey
        {
            CargasSociales = dto.CargasSociales
                .Select(c => new CargaSocial { Nombre = c.Nombre.Trim(), Porcentaje = c.Porcentaje }).ToList(),
            TramosRenta = tramos
                .Select(t => new TramoRenta { Desde = t.Desde, Hasta = t.Hasta, Porcentaje = t.Porcentaje }).ToList(),
            PreavisoTramos = preaviso
                .Select(t => new TramoDias { MesesDesde = t.MesesDesde, MesesHasta = t.MesesHasta, Dias = t.Dias }).ToList(),
            CesantiaMenorUnAnio = cesantiaMenor
                .Select(t => new TramoDias { MesesDesde = t.MesesDesde, MesesHasta = t.MesesHasta, Dias = t.Dias }).ToList(),
            CesantiaPorAnio = dto.CesantiaPorAnio.OrderBy(x => x.Anios)
                .Select(x => new DiasCesantiaAnio { Anios = x.Anios, DiasPorAnio = x.DiasPorAnio }).ToList(),
            FechaActualizacion = DateTime.UtcNow,
            ActualizadoPorId = usuarioId
        });

        return Resultado.Exito();
    }

    private static string? ValidarTramosMeses(List<TramoDiasDto> tramos, string nombre)
    {
        for (var i = 0; i < tramos.Count; i++)
        {
            var t = tramos[i];

            if (t.MesesHasta != null && t.MesesHasta <= t.MesesDesde)
                return $"En la tabla de {nombre}, cada tramo necesita un 'mesesHasta' mayor que 'mesesDesde'.";

            if (i > 0 && (tramos[i - 1].MesesHasta == null || tramos[i - 1].MesesHasta > t.MesesDesde))
                return $"En la tabla de {nombre} hay tramos que se traslapan.";
        }
        return null;
    }
}