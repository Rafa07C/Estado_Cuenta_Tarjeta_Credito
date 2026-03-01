using CreditCardStatement.Core.DTOs;
using MediatR;

namespace CreditCardStatement.Api.CQRS.Queries;

public class GetStatementQuery : IRequest<StatementDto?>
{
    public int CardId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }

    public GetStatementQuery(int cardId, int month, int year)
    {
        CardId = cardId;
        Month = month;
        Year = year;
    }
}