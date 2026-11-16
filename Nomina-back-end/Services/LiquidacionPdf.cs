using System.Globalization;
using Nomina_back_end.DTOs;
using Nomina_back_end.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Nomina_back_end.Services;

public static class LiquidacionPdf
{
    private static readonly CultureInfo CR = CultureInfo.GetCultureInfo("es-CR");
    private const string Empresa = "TechSolutions S.A.";

    public static byte[] Generar(LiquidacionDto l)
    {
        return Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(t => t.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(left =>
                        {
                            left.Item().Text(Empresa).FontSize(16).Bold();
                            left.Item().Text("Comprobante de liquidación").FontSize(12);
                        });

                        row.RelativeItem().AlignRight().Column(right =>
                        {
                            right.Item().Text($"Estado: {l.Estado}").Bold();
                            right.Item().Text($"Fecha de salida: {l.FechaSalida}");
                        });
                    });

                    col.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                });

                page.Content().PaddingVertical(15).Column(col =>
                {
                    col.Spacing(12);

                    if (l.Estado == EstadosLiquidacion.Borrador)
                        col.Item().Text("BORRADOR: este documento aún no está aprobado.")
                            .Bold().FontColor(Colors.Red.Medium);

                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(x => { x.RelativeColumn(); x.RelativeColumn(); });
                        Dato(t, "Empleado", l.NombreEmpleado);
                        Dato(t, "Identificación", l.Identificacion);
                        Dato(t, "Departamento", l.Departamento);
                        Dato(t, "Puesto", l.Puesto);
                        Dato(t, "Fecha de ingreso", l.FechaIngreso);
                        Dato(t, "Antigüedad", l.Antiguedad);
                        Dato(t, "Salario base mensual", Dinero(l.SalarioBase));
                        Dato(t, "Motivo", l.Motivo);
                    });

                    col.Item().Element(e => Seccion(e, "Ingresos", l.Ingresos, "Total ingresos", l.TotalIngresos));
                    col.Item().Element(e => Seccion(e, "Deducciones", l.Deducciones, "Total deducciones", l.TotalDeducciones));

                    col.Item().Background(Colors.Grey.Lighten3).Padding(8).Row(row =>
                    {
                        row.RelativeItem().Text("TOTAL A PAGAR").Bold().FontSize(12);
                        row.ConstantItem(160).AlignRight().Text(Dinero(l.TotalNeto)).Bold().FontSize(12);
                    });

                    col.Item().PaddingTop(40).Row(row =>
                    {
                        row.RelativeItem().PaddingRight(20).Column(c =>
                        {
                            c.Item().LineHorizontal(1);
                            c.Item().AlignCenter().Text("Firma del empleado").FontSize(9);
                        });

                        row.RelativeItem().PaddingLeft(20).Column(c =>
                        {
                            c.Item().LineHorizontal(1);
                            c.Item().AlignCenter().Text("Firma de RR.HH.").FontSize(9);
                        });
                    });
                });

                page.Footer().AlignCenter()
                    .Text("Documento generado por NóminaPro 2026")
                    .FontSize(8).FontColor(Colors.Grey.Medium);
            });
        }).GeneratePdf();
    }

    private static void Seccion(IContainer c, string titulo, List<RubroDto> rubros, string textoTotal, decimal total)
    {
        c.Column(col =>
        {
            col.Item().Background(Colors.Grey.Lighten3).Padding(4).Text(titulo).Bold();

            col.Item().Table(t =>
            {
                t.ColumnsDefinition(x => { x.RelativeColumn(4); x.RelativeColumn(1.5f); });

                foreach (var r in rubros)
                {
                    t.Cell().PaddingVertical(3).Column(cc =>
                    {
                        cc.Item().Text(r.Concepto);
                        cc.Item().Text(r.Detalle).FontSize(8).FontColor(Colors.Grey.Darken1);
                    });

                    t.Cell().PaddingVertical(3).AlignRight().Text(Dinero(r.Monto));
                }

                t.Cell().BorderTop(1).BorderColor(Colors.Grey.Medium).PaddingVertical(3).Text(textoTotal).Bold();
                t.Cell().BorderTop(1).BorderColor(Colors.Grey.Medium).PaddingVertical(3)
                    .AlignRight().Text(Dinero(total)).Bold();
            });
        });
    }

    private static void Dato(TableDescriptor t, string etiqueta, string valor) =>
        t.Cell().PaddingVertical(2).Text(x =>
        {
            x.Span($"{etiqueta}: ").SemiBold();
            x.Span(valor);
        });

    private static string Dinero(decimal monto) => "CRC " + monto.ToString("N2", CR);
}