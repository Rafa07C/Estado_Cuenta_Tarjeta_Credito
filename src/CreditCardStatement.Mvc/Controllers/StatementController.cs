using CreditCardStatement.Mvc.Services;
using CreditCardStatement.Mvc.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CreditCardStatement.Mvc.Controllers;

public class StatementController : Controller
{
    private readonly ApiClient _apiClient;
    private readonly PdfService _pdfService;

    public StatementController(ApiClient apiClient, PdfService pdfService)
    {
        _apiClient = apiClient;
        _pdfService = pdfService;
    }

    public async Task<IActionResult> Index(int cardId = 1, int month = 0, int year = 0)
    {
        if (month == 0) month = DateTime.Now.Month;
        if (year == 0) year = DateTime.Now.Year;

        var vm = new StatementViewModel
        {
            CardId = cardId,
            SelectedMonth = month,
            SelectedYear = year
        };

        try
        {
            vm.Statement = await _apiClient.GetStatementAsync(cardId, year, month);
            vm.Transactions = await _apiClient.GetMonthTransactionsAsync(cardId, year, month);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "No se pudo cargar el estado de cuenta. " + ex.Message;
        }

        return View(vm);
    }

    public async Task<IActionResult> ExportPdf(int cardId = 1, int month = 0, int year = 0)
    {
        if (month == 0) month = DateTime.Now.Month;
        if (year == 0) year = DateTime.Now.Year;

        try
        {
            var statement = await _apiClient.GetStatementAsync(cardId, year, month);
            var transactions = await _apiClient.GetMonthTransactionsAsync(cardId, year, month);

            if (statement == null)
            {
                TempData["Error"] = "No se encontró información para exportar.";
                return RedirectToAction("Index");
            }

            var monthName = new System.Globalization.CultureInfo("es-HN")
                .DateTimeFormat.GetMonthName(month);

            var html = GenerateStatementHtml(statement, transactions, monthName, year);
            var pdf = _pdfService.GenerateStatementPdf(html);

            return File(pdf, "application/pdf", $"EstadoCuenta_{monthName}_{year}.pdf");
        }
        catch (Exception ex)
        {
            TempData["Error"] = "No se pudo generar el PDF. " + ex.Message;
            return RedirectToAction("Index");
        }
    }

    private static string GenerateStatementHtml(
        CreditCardStatement.Core.DTOs.StatementDto s,
        IEnumerable<CreditCardStatement.Core.DTOs.TransactionDto> transactions,
        string monthName, int year)
    {
        var txRows = string.Join("", transactions.Select(tx => $@"
            <tr>
                <td>{tx.TxDate:dd/MM/yyyy}</td>
                <td>{tx.Description ?? "-"}</td>
                <td>{(tx.TxType == "PURCHASE" ? "Compra" : "Pago")}</td>
                <td style='text-align:right'>{tx.Amount:C}</td>
            </tr>"));

        return $@"
        <!DOCTYPE html>
        <html lang='es'>
        <head>
            <meta charset='utf-8'/>
            <style>
                body {{ font-family: Arial, sans-serif; font-size: 12px; color: #333; }}
                h1 {{ color: #c0392b; }}
                h2 {{ color: #c0392b; font-size: 14px; border-bottom: 2px solid #c0392b; padding-bottom: 4px; }}
                table {{ width: 100%; border-collapse: collapse; margin-bottom: 20px; }}
                th {{ background-color: #c0392b; color: white; padding: 8px; text-align: left; }}
                td {{ padding: 6px 8px; border-bottom: 1px solid #ddd; }}
                .text-right {{ text-align: right; }}
                .summary-box {{ background: #f9f9f9; padding: 10px; border-radius: 4px; margin-bottom: 20px; }}
                .row {{ display: flex; justify-content: space-between; }}
                .col {{ width: 48%; }}
            </style>
        </head>
        <body>
            <h1>Banco</h1>
            <p>Estado de Cuenta — {monthName} {year}</p>

            <h2>Información de la Tarjeta</h2>
            <div class='summary-box'>
                <div class='row'>
                    <div class='col'>
                        <p><strong>Titular:</strong> {s.CardHolderName}</p>
                        <p><strong>Tarjeta:</strong> **** **** **** {s.CardNumberLast4}</p>
                    </div>
                    <div class='col'>
                        <p><strong>Límite de Crédito:</strong> {s.CreditLimit:C}</p>
                        <p><strong>Saldo Disponible:</strong> {s.AvailableBalance:C}</p>
                    </div>
                </div>
            </div>

            <h2>Resumen de Pagos</h2>
            <table>
                <tr><td>Saldo Actual</td><td class='text-right'>{s.CurrentBalance:C}</td></tr>
                <tr><td>Compras Este Mes</td><td class='text-right'>{s.PurchasesThisMonth:C}</td></tr>
                <tr><td>Compras Mes Anterior</td><td class='text-right'>{s.PurchasesPreviousMonth:C}</td></tr>
                <tr><td>Tasa de Interés</td><td class='text-right'>{s.InterestRate * 100:F2}%</td></tr>
                <tr><td>Interés Bonificable</td><td class='text-right'>{s.InterestBonificable:C}</td></tr>
                <tr><td>Cuota Mínima a Pagar</td><td class='text-right'>{s.MinimumPayment:C}</td></tr>
                <tr><td><strong>Monto Total a Pagar</strong></td><td class='text-right'><strong>{s.TotalToPay:C}</strong></td></tr>
                <tr><td><strong>Total de Contado con Intereses</strong></td><td class='text-right'><strong>{s.TotalToPayWithInterest:C}</strong></td></tr>
            </table>

            <h2>Movimientos del Mes</h2>
            <table>
                <thead>
                    <tr>
                        <th>Fecha</th>
                        <th>Descripción</th>
                        <th>Tipo</th>
                        <th style='text-align:right'>Monto</th>
                    </tr>
                </thead>
                <tbody>
                    {(string.IsNullOrEmpty(txRows) ? "<tr><td colspan='4'>No hay movimientos para este período.</td></tr>" : txRows)}
                </tbody>
            </table>

            <p style='text-align:center; color:#999; font-size:10px'>
                Generado el {DateTime.Now:dd/MM/yyyy HH:mm} — Banco
            </p>
        </body>
        </html>";
    }
}