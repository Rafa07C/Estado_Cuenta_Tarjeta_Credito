namespace CreditCardStatement.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IStatementRepository Statements { get; }
    ITransactionRepository Transactions { get; }
    Task<int> CommitAsync();
}