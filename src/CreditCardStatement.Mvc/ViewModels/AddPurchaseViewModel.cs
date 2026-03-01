namespace CreditCardStatement.Mvc.ViewModels;

public class AddPurchaseViewModel
{
    public int CardId { get; set; } = 1;
    public DateTime TxDate { get; set; } = DateTime.Now;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}