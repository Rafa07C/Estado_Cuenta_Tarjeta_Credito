using CreditCardStatement.Core.DTOs;
using FluentValidation;

namespace CreditCardStatement.Core.Validators;

public class AddPurchaseValidator : AbstractValidator<AddPurchaseDto>
{
    public AddPurchaseValidator()
    {
        RuleFor(x => x.CardId)
            .GreaterThan(0).WithMessage("CardId must be greater than 0.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(200).WithMessage("Description cannot exceed 200 characters.");

        RuleFor(x => x.TxDate)
            .NotEmpty().WithMessage("Transaction date is required.")
            .LessThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("Transaction date cannot be in the future.");
    }
}