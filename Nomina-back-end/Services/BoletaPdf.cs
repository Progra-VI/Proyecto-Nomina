using System.Globalization;
using Nomina_back_end.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Nomina_back_end.Services;

public static class BoletaPdf
{
    private static readonly CultureInfo CR = CultureInfo.GetCultureInfo("es-CR");
    private const string Empresa = "TechSolutions S.A.";

    // Una página por boleta: sirve para una sola o para toda la planilla
    public static byte[] Generar(IEnumerable<BoletaDto> boletas)
    {
        var lista = boletas.ToList();

        return Document.Create(doc =>
        {
            foreach (var b in lista)
            {
                doc.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(t => t.FontSize(10));

                    page.Header().Element(c => Encabezado(c, b));
                    page.Content().PaddingVertical(15).Element(c => Cuerpo(c, b));
                    page.Footer().AlignCenter()
                        .Text("Documento generado por NóminaPro 2026")
                        .FontSize(8).FontColor(Colors.Grey.Medium);
                });
            }
        }).GeneratePdf();
    }

    private static void Encabezado(IContainer c, BoletaDto b)
    {
        c.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(left =>
                {
                    left.Item().Text(Empresa).FontSize(16).Bold();
                    left.Item().Text("Boleta de pago").FontSize(12);
                });

                row.RelativeItem().AlignRight().Column(right =>
                {
                    right.Item().Text(Periodo(b)).Bold();
                    right.Item().Text($"Del {b.Desde} al {b.Hasta}");
                    right.Item().Text($"Estado de la planilla: {b.EstadoPlanilla}");
                });
            });

            col.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Medium);
        });
    }

    private static void Cuerpo(IContainer c, BoletaDto b)
    {
        c.Column(col =>
        {
            col.Spacing(12);

            col.Item().Table(t =>
            {
                t.ColumnsDefinition(x => { x.RelativeColumn(); x.RelativeColumn(); });
                Dato(t, "Empleado", b.NombreEmpleado);
                Dato(t, "Identificación", b.Identificacion);
                Dato(t, "Departamento", b.Departamento);
                Dato(t, "Puesto", b.Puesto);
                Dato(t, "Salario base mensual", Dinero(b.SalarioBase));
                Dato(t, "Ausencias injustificadas", b.AusenciasInjustificadas.ToString());
            });

            col.Item().Element(e => Seccion(e, "Ingresos", t =>
            {
                Fila(t, $"Salario ({b.DiasLaborados.ToString("0.##", CR)} días)", b.IngresoSalario);

                if (b.IngresoVacaciones > 0)
                    Fila(t, $"Vacaciones disfrutadas ({b.DiasVacaciones.ToString("0.##", CR)} días)", b.IngresoVacaciones);

                if (b.IngresoHorasExtra > 0)
                    Fila(t, $"Horas extra ({b.HorasExtra.ToString("0.##", CR)} h)", b.IngresoHorasExtra);

                Total(t, "Total ingresos", b.TotalIngresos);
            }));

            col.Item().Element(e => Seccion(e, "Deducciones", t =>
            {
                if (b.DeduccionAusencias > 0)
                    Fila(t, $"Ausencias injustificadas ({b.AusenciasInjustificadas})", b.DeduccionAusencias);

                if (b.DeduccionPermisos > 0)
                    Fila(t, "Permisos sin goce de salario", b.DeduccionPermisos);

                foreach (var carga in b.CargasSociales)
                    Fila(t, $"{carga.Nombre} ({carga.Porcentaje.ToString("0.##", CR)}%)", carga.Monto);

                Fila(t, "Impuesto sobre la renta", b.ImpuestoRenta);
                Total(t, "Total deducciones", b.TotalDeducciones);
            }));

            col.Item().Background(Colors.Grey.Lighten3).Padding(8).Row(row =>
            {
                row.RelativeItem().Text("SALARIO NETO").Bold().FontSize(12);
                row.ConstantItem(160).AlignRight().Text(Dinero(b.SalarioNeto)).Bold().FontSize(12);
            });
        });
    }

    private static void Seccion(IContainer c, string titulo, Action<TableDescriptor> filas)
    {
        c.Column(col =>
        {
            col.Item().Background(Colors.Grey.Lighten3).Padding(4).Text(titulo).Bold();

            col.Item().Table(t =>
            {
                t.ColumnsDefinition(x => { x.RelativeColumn(4); x.RelativeColumn(1.5f); });
                filas(t);
            });
        });
    }

    private static void Dato(TableDescriptor t, string etiqueta, string valor) =>
        t.Cell().PaddingVertical(2).Text(x =>
        {
            x.Span($"{etiqueta}: ").SemiBold();
            x.Span(valor);
        });

    private static void Fila(TableDescriptor t, string concepto, decimal monto)
    {
        t.Cell().PaddingVertical(2).Text(concepto);
        t.Cell().PaddingVertical(2).AlignRight().Text(Dinero(monto));
    }

    private static void Total(TableDescriptor t, string concepto, decimal monto)
    {
        t.Cell().BorderTop(1).BorderColor(Colors.Grey.Medium).PaddingVertical(3).Text(concepto).Bold();
        t.Cell().BorderTop(1).BorderColor(Colors.Grey.Medium).PaddingVertical(3)
            .AlignRight().Text(Dinero(monto)).Bold();
    }

    private static string Dinero(decimal monto) => "CRC " + monto.ToString("N2", CR);

    private static string Periodo(BoletaDto b)
    {
        var mes = new DateTime(b.Anio, b.Mes, 1).ToString("MMMM yyyy", CR);
        mes = char.ToUpper(mes[0]) + mes[1..];
        return b.Quincena == null ? $"Planilla de {mes}" : $"Planilla de {mes}, quincena {b.Quincena}";
    }
}