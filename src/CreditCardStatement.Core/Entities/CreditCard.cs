namespace CreditCardStatement.Core.Entities;

public class CreditCard
{
    public int CardId { get; set; }
    public int CardHolderId { get; set; }
    public string CardNumberLast4 { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}