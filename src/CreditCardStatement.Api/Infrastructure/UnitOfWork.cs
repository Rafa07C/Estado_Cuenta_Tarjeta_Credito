using CreditCardStatement.Api.Infrastructure.Repositories;
using CreditCardStatement.Core.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CreditCardStatement.Api.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly IDbConnection _db;
    private IStatementRepository? _statements;
    private ITransactionRepository? _transactions;

    public UnitOfWork(IConfiguration configuration)
    {
        _db = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
        _db.Open();
    }

    public IStatementRepository Statements =>
        _statements ??= new StatementRepository(_db);

    public ITransactionRepository Transactions =>
        _transactions ??= new TransactionRepository(_db);

    public async Task<int> CommitAsync()
    {
        // Dapper con SPs no requiere commit explícito en este caso
        // Este método existe para cumplir el contrato de IUnitOfWork
        return await Task.FromResult(0);
    }

    public void Dispose()
    {
        if (_db.State == ConnectionState.Open)
            _db.Close();
        _db.Dispose();
    }
}