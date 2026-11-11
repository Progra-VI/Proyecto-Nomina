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
                .Select(x => new TramoRentaDto { Desde = x.Desde, Hasta = x.Hasta, Porcentaje = x.Porcentaje }).ToList() ?? new()
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

        await _repo.Guardar(new ConfiguracionLey
        {
            CargasSociales = dto.CargasSociales
                .Select(c => new CargaSocial { Nombre = c.Nombre.Trim(), Porcentaje = c.Porcentaje }).ToList(),
            TramosRenta = tramos
                .Select(t => new TramoRenta { Desde = t.Desde, Hasta = t.Hasta, Porcentaje = t.Porcentaje }).ToList(),
            FechaActualizacion = DateTime.UtcNow,
            ActualizadoPorId = usuarioId
        });

        return Resultado.Exito();
    }
}