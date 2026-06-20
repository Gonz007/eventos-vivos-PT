using FluentValidation;

namespace EventBooking.Application.Reservations.Commands.CreateReservation;

public class CreateReservationCommandValidator : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationCommandValidator()
    {
        RuleFor(x => x.EventId)
            .GreaterThan(0).WithMessage("EventId must be a valid positive integer.");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(1).WithMessage("Quantity must be at least 1.");

        RuleFor(x => x.BuyerName)
            .NotEmpty().WithMessage("BuyerName is required.");

        RuleFor(x => x.BuyerEmail)
            .NotEmpty().WithMessage("BuyerEmail is required.")
            .EmailAddress().WithMessage("BuyerEmail must be a valid email address.");
    }
}
