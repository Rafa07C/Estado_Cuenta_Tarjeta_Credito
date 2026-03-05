using Microsoft.AspNetCore.SignalR;

namespace CreditCardStatement.Api.Hubs;

public class TransactionHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }
}