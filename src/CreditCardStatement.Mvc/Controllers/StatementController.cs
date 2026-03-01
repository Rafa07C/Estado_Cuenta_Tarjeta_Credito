using CreditCardStatement.Mvc.Services;
using CreditCardStatement.Mvc.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CreditCardStatement.Mvc.Controllers;

public class StatementController : Controller
{
    private readonly ApiClient _apiClient;

    public StatementController(ApiClient apiClient)
    {
        _apiClient = apiClient;
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
}