namespace CreditCardStatement.Core.DTOs;

public class AddPaymentDto
{
    public int CardId { get; set; }
    public DateTime TxDate { get; set; }
    public decimal Amount { get; set; }
}