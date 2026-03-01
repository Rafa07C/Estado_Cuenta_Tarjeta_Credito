namespace CreditCardStatement.Core.DTOs;

public class StatementDto
{
    public string CardHolderName { get; set; } = string.Empty;
    public int CardId { get; set; }
    public string CardNumberLast4 { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal PurchasesThisMonth { get; set; }
    public decimal PurchasesPreviousMonth { get; set; }
    public decimal InterestRate { get; set; }
    public decimal MinimumPaymentRate { get; set; }
    public decimal InterestBonificable { get; set; }
    public decimal MinimumPayment { get; set; }
    public decimal TotalToPay { get; set; }
    public decimal TotalToPayWithInterest { get; set; }
}