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
        if (!ModelState.IsValid)
            return View(vm);

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

    public async Task<IActionResult> ExportExcel(int cardId = 1, int month = 0, int year = 0)
    {
        if (month == 0) month = DateTime.Now.Month;
        if (year == 0) year = DateTime.Now.Year;

        try
        {
            var transactions = await _apiClient.GetMonthTransactionsAsync(cardId, year, month);
            var purchases = transactions.Where(t => t.TxType == "PURCHASE").ToList();

            var monthName = new System.Globalization.CultureInfo("es-HN")
                .DateTimeFormat.GetMonthName(month);

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("Compras");

            // Título
            ws.Cell(1, 1).Value = $"Compras — {monthName} {year}";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(1, 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.FromHtml("#c0392b");
            ws.Range(1, 1, 1, 4).Merge();

            // Encabezados
            ws.Cell(2, 1).Value = "Fecha";
            ws.Cell(2, 2).Value = "Descripción";
            ws.Cell(2, 3).Value = "Tipo";
            ws.Cell(2, 4).Value = "Monto";

            var headerRange = ws.Range(2, 1, 2, 4);
            headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#c0392b");
            headerRange.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            headerRange.Style.Font.Bold = true;

            // Datos
            int row = 3;
            foreach (var tx in purchases)
            {
                ws.Cell(row, 1).Value = tx.TxDate.ToString("dd/MM/yyyy");
                ws.Cell(row, 2).Value = tx.Description ?? "-";
                ws.Cell(row, 3).Value = "Compra";
                ws.Cell(row, 4).Value = tx.Amount;
                ws.Cell(row, 4).Style.NumberFormat.Format = "$#,##0.00";

                if (row % 2 == 0)
                    ws.Range(row, 1, row, 4).Style.Fill.BackgroundColor =
                        ClosedXML.Excel.XLColor.FromHtml("#f9f9f9");

                row++;
            }

            // Total
            ws.Cell(row, 3).Value = "Total:";
            ws.Cell(row, 3).Style.Font.Bold = true;
            ws.Cell(row, 4).Value = purchases.Sum(t => t.Amount);
            ws.Cell(row, 4).Style.NumberFormat.Format = "$#,##0.00";
            ws.Cell(row, 4).Style.Font.Bold = true;

            // Ajustar ancho de columnas
            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Compras_{monthName}_{year}.xlsx"
            );
        }
        catch (Exception ex)
        {
            TempData["Error"] = "No se pudo exportar el Excel. " + ex.Message;
            return RedirectToAction("Index");
        }
    }
}