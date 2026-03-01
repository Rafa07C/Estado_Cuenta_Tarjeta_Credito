using CreditCardStatement.Core.DTOs;

namespace CreditCardStatement.Mvc.ViewModels;

public class TransactionViewModel
{
    public IEnumerable<TransactionDto> Transactions { get; set; } = Enumerable.Empty<TransactionDto>();
    public int SelectedMonth { get; set; } = DateTime.Now.Month;
    public int SelectedYear { get; set; } = DateTime.Now.Year;
    public int CardId { get; set; } = 1;
}