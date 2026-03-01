namespace CreditCardStatement.Core.Entities;

public class CardHolder
{
    public int CardHolderId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}