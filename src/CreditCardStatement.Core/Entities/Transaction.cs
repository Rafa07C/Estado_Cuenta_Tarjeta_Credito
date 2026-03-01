namespace CreditCardStatement.Core.Entities;

public class Transaction
{
    public long TransactionId { get; set; }
    public int CardId { get; set; }
    public DateTime TxDate { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public string TxType { get; set; } = string.Empty; // "PURCHASE" or "PAYMENT"
    public DateTime CreatedAt { get; set; }
}