using CreditCardStatement.Core.DTOs;
using CreditCardStatement.Core.Interfaces;
using Dapper;
using System.Data;

namespace CreditCardStatement.Api.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly IDbConnection _db;

    public TransactionRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task AddPurchaseAsync(AddPurchaseDto dto)
    {
        await _db.ExecuteAsync(
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
        await _db.ExecuteAsync(
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