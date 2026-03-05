using CreditCardStatement.Api.Hubs;
using CreditCardStatement.Core.DTOs;
using CreditCardStatement.Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace CreditCardStatement.Api.CQRS.Commands;

public class AddPurchaseHandler : IRequestHandler<AddPurchaseCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubContext<TransactionHub> _hubContext;

    public AddPurchaseHandler(IUnitOfWork unitOfWork, IHubContext<TransactionHub> hubContext)
    {
        _unitOfWork = unitOfWork;
        _hubContext = hubContext;
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

        // Notificar a todos los clientes conectados
        await _hubContext.Clients.All.SendAsync("NuevaTransaccion", new
        {
            cardId = request.CardId,
            txDate = request.TxDate.ToString("dd/MM/yyyy"),
            description = request.Description,
            amount = request.Amount,
            txType = "PURCHASE"
        });

        return Unit.Value;
    }
}