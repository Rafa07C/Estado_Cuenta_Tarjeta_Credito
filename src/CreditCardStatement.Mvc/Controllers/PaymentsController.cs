using CreditCardStatement.Core.DTOs;
using CreditCardStatement.Mvc.Services;
using CreditCardStatement.Mvc.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CreditCardStatement.Mvc.Controllers;

public class PaymentsController : Controller
{
    private readonly ApiClient _apiClient;

    public PaymentsController(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public IActionResult Index()
    {
        return View(new AddPaymentViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Index(AddPaymentViewModel vm)
    {
        var dto = new AddPaymentDto
        {
            CardId = vm.CardId,
            TxDate = vm.TxDate,
            Amount = vm.Amount
        };

        var (success, message) = await _apiClient.AddPaymentAsync(dto);

        if (success)
        {
            TempData["Success"] = message;
            return RedirectToAction("Index", "Statement");
        }

        TempData["Error"] = message;
        return View(vm);
    }
}