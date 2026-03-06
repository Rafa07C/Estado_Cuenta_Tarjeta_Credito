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

            var pdf = _pdfService.GenerateStatementPdf(statement, transactions, monthName, year);

            return File(pdf, "application/pdf", $"EstadoCuenta_{monthName}_{year}.pdf");
        }
        catch (Exception ex)
        {
            TempData["Error"] = "No se pudo generar el PDF. " + ex.Message;
            return RedirectToAction("Index");
        }
    }
}