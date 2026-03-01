using CreditCardStatement.Core.DTOs;
using CreditCardStatement.Core.Interfaces;
using MediatR;

namespace CreditCardStatement.Api.CQRS.Commands;

public class AddPaymentHandler : IRequestHandler<AddPaymentCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddPaymentHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(AddPaymentCommand request, CancellationToken cancellationToken)
    {
        var dto = new AddPaymentDto
        {
            CardId = request.CardId,
            TxDate = request.TxDate,
            Amount = request.Amount
        };

        await _unitOfWork.Transactions.AddPaymentAsync(dto);
        await _unitOfWork.CommitAsync();
        return Unit.Value;
    }
}