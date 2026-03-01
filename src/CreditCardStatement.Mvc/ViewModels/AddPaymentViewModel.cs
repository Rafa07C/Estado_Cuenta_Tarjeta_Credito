namespace CreditCardStatement.Mvc.ViewModels;

public class AddPaymentViewModel
{
    public int CardId { get; set; } = 1;
    public DateTime TxDate { get; set; } = DateTime.Now;
    public decimal Amount { get; set; }
}