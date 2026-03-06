using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using CreditCardStatement.Core.DTOs;

namespace CreditCardStatement.Mvc.Services;

public class PdfService
{
    public PdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateStatementPdf(
        StatementDto s,
        IEnumerable<TransactionDto> transactions,
        string monthName,
        int year)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Banco")
                            .FontSize(20).Bold().FontColor("#c0392b");
                        row.ConstantItem(200).AlignRight()
                            .Text($"Estado de Cuenta — {monthName} {year}")
                            .FontSize(10).FontColor("#666");
                    });
                    col.Item().PaddingTop(4).LineHorizontal(1).LineColor("#c0392b");
                });

                page.Content().PaddingTop(10).Column(col =>
                {
                    // Información de la tarjeta
                    col.Item().Background("#f9f9f9").Padding(8).Column(info =>
                    {
                        info.Item().Text("Información de la Tarjeta")
                            .FontSize(12).Bold().FontColor("#c0392b");
                        info.Item().PaddingTop(4).Row(row =>
                        {
                            row.RelativeItem().Column(left =>
                            {
                                left.Item().Text($"Titular: {s.CardHolderName}").Bold();
                                left.Item().Text($"Tarjeta: **** **** **** {s.CardNumberLast4}");
                            });
                            row.RelativeItem().Column(right =>
                            {
                                right.Item().Text($"Límite de Crédito: {s.CreditLimit:C}").Bold();
                                right.Item().Text($"Saldo Disponible: {s.AvailableBalance:C}");
                            });
                        });
                    });

                    col.Item().PaddingTop(10).Text("Resumen de Pagos")
                        .FontSize(12).Bold().FontColor("#c0392b");
                    col.Item().LineHorizontal(1).LineColor("#c0392b");

                    // Tabla resumen
                    col.Item().PaddingTop(4).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn();
                            cols.ConstantColumn(120);
                        });

                        void AddRow(string label, string value, bool bold = false)
                        {
                            if (bold)
                            {
                                table.Cell().Padding(4).Text(label).Bold();
                                table.Cell().Padding(4).AlignRight().Text(value).Bold();
                            }
                            else
                            {
                                table.Cell().Padding(4).Text(label);
                                table.Cell().Padding(4).AlignRight().Text(value);
                            }
                        }

                        AddRow("Saldo Actual", s.CurrentBalance.ToString("C"));
                        AddRow("Compras Este Mes", s.PurchasesThisMonth.ToString("C"));
                        AddRow("Compras Mes Anterior", s.PurchasesPreviousMonth.ToString("C"));
                        AddRow("Tasa de Interés", $"{s.InterestRate * 100:F2}%");
                        AddRow("Interés Bonificable", s.InterestBonificable.ToString("C"));
                        AddRow("Cuota Mínima a Pagar", s.MinimumPayment.ToString("C"));
                        AddRow("Monto Total a Pagar", s.TotalToPay.ToString("C"), bold: true);
                        AddRow("Total de Contado con Intereses", s.TotalToPayWithInterest.ToString("C"), bold: true);
                    });

                    col.Item().PaddingTop(10).Text("Movimientos del Mes")
                        .FontSize(12).Bold().FontColor("#c0392b");
                    col.Item().LineHorizontal(1).LineColor("#c0392b");

                    // Tabla transacciones
                    col.Item().PaddingTop(4).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(80);
                            cols.RelativeColumn();
                            cols.ConstantColumn(70);
                            cols.ConstantColumn(90);
                        });

                        // Header
                        foreach (var h in new[] { "Fecha", "Descripción", "Tipo", "Monto" })
                        {
                            table.Cell().Background("#c0392b").Padding(6)
                                .Text(h).FontColor(Colors.White).Bold();
                        }

                        var txList = transactions.ToList();
                        if (!txList.Any())
                        {
                            table.Cell().ColumnSpan(4).Padding(6)
                                .Text("No hay movimientos para este período.")
                                .FontColor("#999");
                        }
                        else
                        {
                            foreach (var tx in txList)
                            {
                                table.Cell().Padding(5).Text(tx.TxDate.ToString("dd/MM/yyyy"));
                                table.Cell().Padding(5).Text(tx.Description ?? "-");
                                table.Cell().Padding(5).Text(tx.TxType == "PURCHASE" ? "Compra" : "Pago");
                                table.Cell().Padding(5).AlignRight().Text(tx.Amount.ToString("C"));
                            }
                        }
                    });
                });

                page.Footer().AlignCenter()
                    .Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm} — Banco")
                    .FontSize(8).FontColor("#999");
            });
        }).GeneratePdf();
    }
}