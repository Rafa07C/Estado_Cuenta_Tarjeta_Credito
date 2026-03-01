using CreditCardStatement.Core.DTOs;

namespace CreditCardStatement.Core.Interfaces;

public interface IStatementRepository
{
    Task<StatementDto?> GetStatementAsync(int cardId, int month, int year);
    Task<IEnumerable<TransactionDto>> GetMonthTransactionsAsync(int cardId, int month, int year);
}