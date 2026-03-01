using CreditCardStatement.Core.DTOs;

namespace CreditCardStatement.Core.Interfaces;

public interface ITransactionRepository
{
    Task AddPurchaseAsync(AddPurchaseDto dto);
    Task AddPaymentAsync(AddPaymentDto dto);
}