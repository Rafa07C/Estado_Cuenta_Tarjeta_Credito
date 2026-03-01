using CreditCardStatement.Api.CQRS.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CreditCardStatement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatementController : ControllerBase
{
    private readonly IMediator _mediator;

    public StatementController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Gets the credit card statement for a given month and year.</summary>
    [HttpGet("{cardId}/{year}/{month}")]
    public async Task<IActionResult> GetStatement(int cardId, int year, int month)
    {
        var result = await _mediator.Send(new GetStatementQuery(cardId, month, year));

        if (result is null)
            return NotFound(new { Message = $"No se encontró información para esta tarjeta." });

        return Ok(result);
    }
}