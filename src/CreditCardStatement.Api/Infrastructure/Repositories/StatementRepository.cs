using CreditCardStatement.Core.DTOs;
using CreditCardStatement.Core.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CreditCardStatement.Api.Infrastructure.Repositories;

public class StatementRepository : IStatementRepository
{
    private readonly IDbConnection _db;

    public StatementRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<StatementDto?> GetStatementAsync(int cardId, int month, int year)
    {
        var result = await _db.QueryFirstOrDefaultAsync<StatementDto>(
            "dbo.sp_GetStatement",
            new { CardId = cardId, Month = month, Year = year },
            commandType: CommandType.StoredProcedure
        );
        return result;
    }

    public async Task<IEnumerable<TransactionDto>> GetMonthTransactionsAsync(int cardId, int month, int year)
    {
        var result = await _db.QueryAsync<TransactionDto>(
            "dbo.sp_GetMonthTransactions",
            new { CardId = cardId, Month = month, Year = year },
            commandType: CommandType.StoredProcedure
        );
        return result;
    }
}