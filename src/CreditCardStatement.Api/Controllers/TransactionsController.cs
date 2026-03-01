using CreditCardStatement.Api.CQRS.Commands;
using CreditCardStatement.Api.CQRS.Queries;
using CreditCardStatement.Core.DTOs;
using CreditCardStatement.Core.Validators;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CreditCardStatement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Gets all transactions for a card in a given month and year.</summary>
    [HttpGet("{cardId}/{year}/{month}")]
    public async Task<IActionResult> GetMonthTransactions(int cardId, int year, int month)
    {
        var result = await _mediator.Send(new GetMonthTransactionsQuery(cardId, month, year));
        return Ok(result);
    }

    /// <summary>Adds a new purchase to the credit card.</summary>
    [HttpPost("purchase")]
    public async Task<IActionResult> AddPurchase([FromBody] AddPurchaseDto dto)
    {
        var validator = new AddPurchaseValidator();
        var validation = await validator.ValidateAsync(dto);

        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

        await _mediator.Send(new AddPurchaseCommand
        {
            CardId = dto.CardId,
            TxDate = dto.TxDate,
            Description = dto.Description,
            Amount = dto.Amount
        });

        return Ok(new { Message = "Compra registrada exitosamente." });
    }

    /// <summary>Adds a new payment to the credit card.</summary>
    [HttpPost("payment")]
    public async Task<IActionResult> AddPayment([FromBody] AddPaymentDto dto)
    {
        var validator = new AddPaymentValidator();
        var validation = await validator.ValidateAsync(dto);

        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

        await _mediator.Send(new AddPaymentCommand
        {
            CardId = dto.CardId,
            TxDate = dto.TxDate,
            Amount = dto.Amount
        });

        return Ok(new { Message = "Pago registrado exitosamente." });
    }
}