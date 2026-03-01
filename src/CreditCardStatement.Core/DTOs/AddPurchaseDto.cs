namespace CreditCardStatement.Core.DTOs;

public class AddPurchaseDto
{
    public int CardId { get; set; }
    public DateTime TxDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}