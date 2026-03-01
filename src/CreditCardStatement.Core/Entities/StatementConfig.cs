namespace CreditCardStatement.Core.Entities;

public class StatementConfig
{
    public int ConfigId { get; set; }
    public decimal InterestRate { get; set; }
    public decimal MinimumPaymentRate { get; set; }
    public DateTime UpdatedAt { get; set; }
}