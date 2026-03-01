using CreditCardStatement.Core.DTOs;
using CreditCardStatement.Core.Interfaces;
using MediatR;

namespace CreditCardStatement.Api.CQRS.Queries;

public class GetStatementHandler : IRequestHandler<GetStatementQuery, StatementDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetStatementHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<StatementDto?> Handle(GetStatementQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Statements.GetStatementAsync(request.CardId, request.Month, request.Year);
    }
}