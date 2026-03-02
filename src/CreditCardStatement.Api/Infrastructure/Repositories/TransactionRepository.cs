using CreditCardStatement.Core.DTOs;
using CreditCardStatement.Core.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CreditCardStatement.Api.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly string _connectionString;

    public TransactionRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task AddPurchaseAsync(AddPurchaseDto dto)
    {
        using var db = new SqlConnection(_connectionString);
        await db.ExecuteAsync(
            "dbo.sp_AddPurchase",
            new
            {
                CardId = dto.CardId,
                TxDate = dto.TxDate,
                Description = dto.Description,
                Amount = dto.Amount
            },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task AddPaymentAsync(AddPaymentDto dto)
    {
        using var db = new SqlConnection(_connectionString);
        await db.ExecuteAsync(
            "dbo.sp_AddPayment",
            new
            {
                CardId = dto.CardId,
                TxDate = dto.TxDate,
                Amount = dto.Amount
            },
            commandType: CommandType.StoredProcedure
        );
    }
}