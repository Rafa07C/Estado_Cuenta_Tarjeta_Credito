using MediatR;

namespace CreditCardStatement.Api.CQRS.Commands;

public class AddPaymentCommand : IRequest
{
    public int CardId { get; set; }
    public DateTime TxDate { get; set; }
    public decimal Amount { get; set; }
}