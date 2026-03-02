using CreditCardStatement.Core.DTOs;
using CreditCardStatement.Core.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CreditCardStatement.Api.Infrastructure.Repositories;

public class StatementRepository : IStatementRepository
{
    private readonly string _connectionString;

    public StatementRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<StatementDto?> GetStatementAsync(int cardId, int month, int year)
    {
        using var db = new SqlConnection(_connectionString);
        return await db.QueryFirstOrDefaultAsync<StatementDto>(
            "dbo.sp_GetStatement",
            new { CardId = cardId, Month = month, Year = year },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<IEnumerable<TransactionDto>> GetMonthTransactionsAsync(int cardId, int month, int year)
    {
        using var db = new SqlConnection(_connectionString);
        return await db.QueryAsync<TransactionDto>(
            "dbo.sp_GetMonthTransactions",
            new { CardId = cardId, Month = month, Year = year },
            commandType: CommandType.StoredProcedure
        );
    }
}