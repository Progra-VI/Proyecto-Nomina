using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Nomina_back_end;
using Nomina_back_end.Data;
using Nomina_back_end.Repositories;
using Nomina_back_end.Services;
using QuestPDF.Infrastructure;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// ---------- Licencia de QuestPDF (PDF de boletas y liquidaciones) ----------
QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese su token JWT."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});

builder.Services.AddControllers();

// ---------- Repositorios y Servicios ----------
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

builder.Services.AddSingleton<LiquidacionRepository>();
builder.Services.AddSingleton<ComprobanteStorage>();
builder.Services.AddScoped<LiquidacionService>();

builder.Services.AddScoped<AuthService>();

// ---------- Configuración ----------
var mongoSettings = builder.Configuration
    .GetSection("MongoDB")
    .Get<MongoDbSettings>();

if (mongoSettings == null ||
    string.IsNullOrWhiteSpace(mongoSettings.ConnectionString) ||
    string.IsNullOrWhiteSpace(mongoSettings.DatabaseName))
{
    throw new Exception("La configuración de MongoDB no está completa.");
}

var jwt = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtSettings>();

if (jwt == null || string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
{
    throw new Exception("La configuración de JWT no está completa (la clave debe tener 32+ caracteres).");
}

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// ---------- MongoDB ----------
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoSettings.ConnectionString));

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase(mongoSettings.DatabaseName));

builder.Services.AddHealthChecks()
    .AddCheck<MongoDbHealthCheck>("mongodb");

// ---------- Seguridad: JWT y CORS ----------
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

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200") 
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

await AdminSeeder.Sembrar(app.Services, app.Configuration, app.Logger);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/api/health/mongodb");
app.MapControllers();
app.MapGet("/", () => "NominaPro API funcionando");

app.Run();