using Nomina_back_end.Entities;
using Nomina_back_end.Repositories;

namespace Nomina_back_end.Data;

public class SeedAdminSettings
{
    public string Identificacion { get; set; } = string.Empty;
    public string Nombre { get; set; } = "Administrador RR.HH.";
    public string Correo { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Departamento { get; set; } = "Recursos Humanos";
    public decimal SalarioBase { get; set; } = 0;
}

public static class AdminSeeder
{
    public static async Task Sembrar(IServiceProvider services, IConfiguration config, ILogger logger)
    {
        try
        {
            var s = config.GetSection("SeedAdmin").Get<SeedAdminSettings>();

            if (s == null ||
                string.IsNullOrWhiteSpace(s.Correo) ||
                string.IsNullOrWhiteSpace(s.Password) ||
                string.IsNullOrWhiteSpace(s.Identificacion))
            {
                logger.LogWarning("SeedAdmin no está configurado: no se creará el usuario RR.HH. inicial.");
                return;
            }

            if (s.Password.Length < 8)
            {
                logger.LogWarning("SeedAdmin: la contraseña debe tener al menos 8 caracteres.");
                return;
            }

            var usuarios = services.GetRequiredService<UsuarioRepository>();
            var departamentos = services.GetRequiredService<DepartamentoRepository>();

            // Si ya hay un RR.HH. activo, no se hace nada
            var existentes = await usuarios.ObtenerTodos();
            if (existentes.Any(u => u.Rol == Roles.RRHH && u.Estado == EstadosUsuario.Activo))
                return;

            var correo = s.Correo.Trim().ToLower();
            if (await usuarios.ObtenerPorCorreo(correo) != null)
            {
                logger.LogWarning("SeedAdmin: ya existe un usuario con el correo {Correo} que no es RR.HH. activo.", correo);
                return;
            }

            var depto = await departamentos.ObtenerPorNombre(s.Departamento);
            if (depto == null)
            {
                depto = new Departamento
                {
                    Nombre = s.Departamento,
                    Descripcion = "Departamento creado automáticamente al iniciar el sistema"
                };
                await departamentos.Crear(depto);
            }

            await usuarios.Crear(new Usuario
            {
                Identificacion = s.Identificacion.Trim(),
                Nombre = s.Nombre.Trim(),
                Correo = correo,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(s.Password),
                Rol = Roles.RRHH,
                DepartamentoId = depto.Id!,
                Puesto = "Encargado de Nómina",
                FechaIngreso = DateTime.UtcNow.Date,
                SalarioBase = s.SalarioBase,
                TipoJornada = "Diurna",
                Estado = EstadosUsuario.Activo
            });

            logger.LogInformation("SeedAdmin: usuario RR.HH. inicial creado ({Correo}).", correo);
        }
        catch (Exception ex)
        {
            // Si Mongo no responde, la API arranca de todos modos
            logger.LogError(ex, "SeedAdmin: no se pudo crear el usuario RR.HH. inicial.");
        }
    }
}