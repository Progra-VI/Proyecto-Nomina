using MongoDB.Driver;
using Nomina_back_end;
using Nomina_back_end.Data;
using Nomina_back_end.Services;
using Nomina_back_end.Repositories;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddControllers();

builder.Services.AddSingleton<DepartamentoRepository>();
builder.Services.AddScoped<DepartamentoService>();

builder.Services.AddSingleton<UsuarioRepository>();
builder.Services.AddScoped<UsuarioService>();

builder.Services.AddSingleton<TipoPermisoRepository>();
builder.Services.AddScoped<TipoPermisoService>();

builder.Services.AddSingleton<TipoHoraExtraRepository>();
builder.Services.AddScoped<TipoHoraExtraService>();

builder.Services.AddSingleton<MotivoLiquidacionRepository>();
builder.Services.AddScoped<MotivoLiquidacionService>();

builder.Services.AddSingleton<AsistenciaRepository>();
builder.Services.AddScoped<AsistenciaService>();

builder.Services.AddSingleton<FeriadoRepository>();
builder.Services.AddScoped<FeriadoService>();

builder.Services.AddSingleton<PermisoRepository>();
builder.Services.AddScoped<PermisoService>();

builder.Services.AddSingleton<ParametroRepository>();
builder.Services.AddScoped<ParametroService>();

builder.Services.AddSingleton<VacacionRepository>();
builder.Services.AddScoped<VacacionService>();

builder.Services.AddSingleton<HoraExtraRepository>();
builder.Services.AddScoped<HoraExtraService>();

builder.Services.AddSingleton<ConfiguracionLeyRepository>();
builder.Services.AddScoped<ConfiguracionLeyService>();

builder.Services.AddSingleton<PlanillaRepository>();
builder.Services.AddSingleton<BoletaRepository>();
builder.Services.AddScoped<PlanillaService>();

var mongoSettings = builder.Configuration
    .GetSection("MongoDB")
    .Get<MongoDbSettings>();

if (mongoSettings == null ||
    string.IsNullOrWhiteSpace(mongoSettings.ConnectionString) ||
    string.IsNullOrWhiteSpace(mongoSettings.DatabaseName))
{
    throw new Exception("La configuración de MongoDB no está completa.");
}

builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient(mongoSettings.ConnectionString));

builder.Services.AddHealthChecks()
    .AddCheck<MongoDbHealthCheck>("mongodb");

builder.Services.AddSingleton(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoSettings.DatabaseName);
});

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<AuthService>();

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
if (jwt == null || string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
{
    throw new Exception("La configuración de JWT no está completa (la clave debe tener 32+ caracteres).");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var h = ctx.Request.Headers.Authorization.ToString();
                Console.WriteLine($"JWT recibido: {ctx.Request.Method} {ctx.Request.Path} | header: {h.Length} chars | empieza con: '{(h.Length > 15 ? h[..15] : h)}'");
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = ctx =>
            {
                Console.WriteLine("JWT falló: " + ctx.Exception.Message);
                return Task.CompletedTask;
            },
            OnChallenge = ctx =>
            {
                Console.WriteLine("JWT challenge: " + ctx.Error + " - " + ctx.ErrorDescription);
                return Task.CompletedTask;
            }
        };

    });

builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/api/health/mongodb");
app.MapControllers();

app.MapGet("/", () => "NominaPro API funcionando");

app.Run();