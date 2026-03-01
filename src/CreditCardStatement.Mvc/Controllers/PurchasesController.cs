using CreditCardStatement.Core.DTOs;
using CreditCardStatement.Mvc.Services;
using CreditCardStatement.Mvc.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CreditCardStatement.Mvc.Controllers;

public class PurchasesController : Controller
{
    private readonly ApiClient _apiClient;

    public PurchasesController(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public IActionResult Index()
    {
        return View(new AddPurchaseViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Index(AddPurchaseViewModel vm)
    {
        var dto = new AddPurchaseDto
        {
            CardId = vm.CardId,
            TxDate = vm.TxDate,
            Description = vm.Description,
            Amount = vm.Amount
        };

        var (success, message) = await _apiClient.AddPurchaseAsync(dto);

        if (success)
        {
            TempData["Success"] = message;
            return RedirectToAction("Index", "Statement");
        }

        TempData["Error"] = message;
        return View(vm);
    }
}