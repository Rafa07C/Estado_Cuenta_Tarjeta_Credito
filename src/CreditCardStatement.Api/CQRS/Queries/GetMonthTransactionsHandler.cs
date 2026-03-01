using CreditCardStatement.Core.DTOs;
using CreditCardStatement.Core.Interfaces;
using MediatR;

namespace CreditCardStatement.Api.CQRS.Queries;

public class GetMonthTransactionsHandler : IRequestHandler<GetMonthTransactionsQuery, IEnumerable<TransactionDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMonthTransactionsHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<TransactionDto>> Handle(GetMonthTransactionsQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Statements.GetMonthTransactionsAsync(request.CardId, request.Month, request.Year);
    }
}