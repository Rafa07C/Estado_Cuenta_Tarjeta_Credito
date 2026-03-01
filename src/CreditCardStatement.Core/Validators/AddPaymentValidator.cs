using CreditCardStatement.Core.DTOs;
using FluentValidation;

namespace CreditCardStatement.Core.Validators;

public class AddPaymentValidator : AbstractValidator<AddPaymentDto>
{
    public AddPaymentValidator()
    {
        RuleFor(x => x.CardId)
            .GreaterThan(0).WithMessage("Debe seleccionar una tarjeta válida.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El monto del pago debe ser mayor a $0.00.");

        RuleFor(x => x.TxDate)
            .NotEmpty().WithMessage("La fecha del pago es requerida.")
            .LessThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("La fecha del pago no puede ser una fecha futura.");
    }
}