using MongoDB.Driver;
using Nomina_back_end;
using Nomina_back_end.Data;
using Nomina_back_end.Services;
using Nomina_back_end.Repositories;

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapHealthChecks("/api/health/mongodb");

app.MapControllers();

app.MapGet("/", () => "NominaPro API funcionando");

app.Run();