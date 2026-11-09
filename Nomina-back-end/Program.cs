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

builder.Services.AddSingleton<UsuarioRepository>();

builder.Services.AddScoped<UsuarioService>();

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Use(async (ctx, next) =>
{
    var h = ctx.Request.Headers.Authorization.ToString();
    Console.WriteLine($"[{ctx.Request.Path}] Auth header: {h.Length} chars, empieza con: '{(h.Length > 15 ? h[..15] : h)}'");
    await next();
    Console.WriteLine($"[{ctx.Request.Path}] Autenticado: {ctx.User.Identity?.IsAuthenticated} | Status: {ctx.Response.StatusCode}");
});


app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/debug/auth", (HttpContext ctx) => new
{
    headerRecibido = ctx.Request.Headers.Authorization.ToString().Length,
    autenticado = ctx.User.Identity?.IsAuthenticated ?? false,
    claims = ctx.User.Claims.Select(c => c.Type + " = " + c.Value)
}).AllowAnonymous();

app.MapHealthChecks("/api/health/mongodb");

app.MapControllers();

app.MapGet("/", () => "NominaPro API funcionando");

app.Run();