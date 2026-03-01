using CreditCardStatement.Core.DTOs;
using CreditCardStatement.Core.Interfaces;
using MediatR;

namespace CreditCardStatement.Api.CQRS.Commands;

public class AddPurchaseHandler : IRequestHandler<AddPurchaseCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddPurchaseHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(AddPurchaseCommand request, CancellationToken cancellationToken)
    {
        var dto = new AddPurchaseDto
        {
            CardId = request.CardId,
            TxDate = request.TxDate,
            Description = request.Description,
            Amount = request.Amount
        };

        await _unitOfWork.Transactions.AddPurchaseAsync(dto);
        await _unitOfWork.CommitAsync();
        return Unit.Value;
    }
}