using CreditCardStatement.Mvc.Services;
using CreditCardStatement.Mvc.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CreditCardStatement.Mvc.Controllers;

public class TransactionsController : Controller
{
    private readonly ApiClient _apiClient;
    private readonly IConfiguration _configuration;

    public TransactionsController(ApiClient apiClient, IConfiguration configuration)
    {
        _apiClient = apiClient;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index(int cardId = 1, int month = 0, int year = 0)
    {
        if (month == 0) month = DateTime.Now.Month;
        if (year == 0) year = DateTime.Now.Year;

        ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"];

        var vm = new TransactionViewModel
        {
            CardId = cardId,
            SelectedMonth = month,
            SelectedYear = year
        };

        try
        {
            vm.Transactions = await _apiClient.GetMonthTransactionsAsync(cardId, year, month);
        }
        catch (Exception ex)
        {
            TempData["Error"] = "No se pudo cargar el historial. " + ex.Message;
        }

        return View(vm);
    }
}