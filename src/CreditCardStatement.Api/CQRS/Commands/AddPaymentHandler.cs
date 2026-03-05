using CreditCardStatement.Api.Hubs;
using CreditCardStatement.Core.DTOs;
using CreditCardStatement.Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace CreditCardStatement.Api.CQRS.Commands;

public class AddPaymentHandler : IRequestHandler<AddPaymentCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubContext<TransactionHub> _hubContext;

    public AddPaymentHandler(IUnitOfWork unitOfWork, IHubContext<TransactionHub> hubContext)
    {
        _unitOfWork = unitOfWork;
        _hubContext = hubContext;
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

        // Notificar a todos los clientes conectados
        await _hubContext.Clients.All.SendAsync("NuevaTransaccion", new
        {
            cardId = request.CardId,
            txDate = request.TxDate.ToString("dd/MM/yyyy"),
            description = "-",
            amount = request.Amount,
            txType = "PAYMENT"
        });

        return Unit.Value;
    }
}