using CreditCardStatement.Api.Infrastructure.Repositories;
using CreditCardStatement.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace CreditCardStatement.Api.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly string _connectionString;
    private IStatementRepository? _statements;
    private ITransactionRepository? _transactions;

    public UnitOfWork(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public IStatementRepository Statements =>
        _statements ??= new StatementRepository(_connectionString);

    public ITransactionRepository Transactions =>
        _transactions ??= new TransactionRepository(_connectionString);

    public async Task<int> CommitAsync() => await Task.FromResult(0);

    public void Dispose() { }
}