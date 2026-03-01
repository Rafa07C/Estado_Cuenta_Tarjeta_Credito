using MediatR;

namespace CreditCardStatement.Api.CQRS.Commands;

public class AddPurchaseCommand : IRequest
{
    public int CardId { get; set; }
    public DateTime TxDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}