using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Nomina_back_end.Data;

public class MongoDbHealthCheck : IHealthCheck
{
    private readonly IMongoDatabase _database;

    public MongoDbHealthCheck(IMongoDatabase database)
    {
        _database = database;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(10));

            await _database.RunCommandAsync<BsonDocument>(
                new BsonDocument("ping", 1),
                cancellationToken: timeout.Token);

            return HealthCheckResult.Healthy("MongoDB conectado correctamente");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Error de conexión con MongoDB", ex);
        }
    }
}