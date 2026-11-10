using System.Globalization;

namespace Nomina_back_end.Services;

// Costa Rica está en UTC-6 todo el año (no usa horario de verano)
public static class HoraCR
{
    private const int Offset = -6;

    public static string Fecha(DateTime utc) =>
        utc.AddHours(Offset).ToString("yyyy-MM-dd");

    public static DateTime ALocal(DateTime utc) =>
        DateTime.SpecifyKind(utc, DateTimeKind.Utc).AddHours(Offset);

    public static DateTime AUtc(string fecha, string hora)
    {
        var local = DateTime.ParseExact($"{fecha} {hora}", "yyyy-MM-dd HH:mm",
            CultureInfo.InvariantCulture);
        return DateTime.SpecifyKind(local.AddHours(-Offset), DateTimeKind.Utc);
    }

    // 1 = lunes ... 7 = domingo (igual que en el horario del empleado)
    public static int DiaSemana(DateTime fecha) => ((int)fecha.DayOfWeek + 6) % 7 + 1;
}