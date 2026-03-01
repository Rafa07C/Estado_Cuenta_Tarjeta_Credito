namespace CreditCardStatement.Core.DTOs;

public class TransactionDto
{
    public long TransactionId { get; set; }
    public int CardId { get; set; }
    public DateTime TxDate { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public string TxType { get; set; } = string.Empty;
}