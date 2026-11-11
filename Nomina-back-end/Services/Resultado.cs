namespace Nomina_back_end.Services;

public record Resultado(bool Ok, string? Error = null, int Status = 400)
{
    public static Resultado Exito() => new(true);
    public static Resultado Fallo(string error, int status = 400) => new(false, error, status);
}