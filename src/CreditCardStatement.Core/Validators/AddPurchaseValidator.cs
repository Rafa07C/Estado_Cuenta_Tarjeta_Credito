using CreditCardStatement.Core.DTOs;
using FluentValidation;

namespace CreditCardStatement.Core.Validators;

public class AddPurchaseValidator : AbstractValidator<AddPurchaseDto>
{
    public AddPurchaseValidator()
    {
        RuleFor(x => x.CardId)
            .GreaterThan(0).WithMessage("Debe seleccionar una tarjeta válida.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El monto debe ser mayor a $0.00.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La descripción de la compra es requerida.")
            .MaximumLength(200).WithMessage("La descripción no puede exceder 200 caracteres.");

        RuleFor(x => x.TxDate)
            .NotEmpty().WithMessage("La fecha de la compra es requerida.")
            .LessThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("La fecha de la compra no puede ser una fecha futura.");
    }
}