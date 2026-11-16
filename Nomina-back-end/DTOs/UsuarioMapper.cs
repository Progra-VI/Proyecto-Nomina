using Nomina_back_end.Entities;

namespace Nomina_back_end.DTOs;

public static class UsuarioMapper
{
    public static UsuarioRespuestaDto ADto(Usuario u) => new()
    {
        Id = u.Id,
        Identificacion = u.Identificacion,
        Nombre = u.Nombre,
        Correo = u.Correo,
        Rol = u.Rol,
        DepartamentoId = u.DepartamentoId,
        Puesto = u.Puesto,
        JefaturaDirectaId = u.JefaturaDirectaId,
        FechaIngreso = u.FechaIngreso,
        SalarioBase = u.SalarioBase,
        TipoJornada = u.TipoJornada,
        FotoUrl = u.FotoUrl,
        Estado = u.Estado,
        Horario = new HorarioDto
        {
            HoraEntrada = u.Horario.HoraEntrada,
            HoraSalida = u.Horario.HoraSalida,
            DiasLaborales = u.Horario.DiasLaborales
        }
    };
}