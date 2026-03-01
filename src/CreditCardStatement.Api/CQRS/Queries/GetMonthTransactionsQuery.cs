using CreditCardStatement.Core.DTOs;
using MediatR;

namespace CreditCardStatement.Api.CQRS.Queries;

public class GetMonthTransactionsQuery : IRequest<IEnumerable<TransactionDto>>
{
    public int CardId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }

    public GetMonthTransactionsQuery(int cardId, int month, int year)
    {
        CardId = cardId;
        Month = month;
        Year = year;
    }
}