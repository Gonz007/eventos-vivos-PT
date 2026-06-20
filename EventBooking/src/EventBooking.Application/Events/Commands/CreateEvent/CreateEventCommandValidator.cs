using FluentValidation;

namespace EventBooking.Application.Events.Commands.CreateEvent;

public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MinimumLength(5).WithMessage("Title must be at least 5 characters.")
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MinimumLength(10).WithMessage("Description must be at least 10 characters.")
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.VenueId)
            .GreaterThan(0).WithMessage("VenueId must be a valid positive integer.");

        RuleFor(x => x.MaxCapacity)
            .GreaterThan(0).WithMessage("MaxCapacity must be a positive integer.");

        RuleFor(x => x.StartDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("StartDate must be in the future.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("EndDate must be after StartDate.");

        RuleFor(x => x.TicketPrice)
            .GreaterThan(0).WithMessage("TicketPrice must be a positive decimal.");

        RuleFor(x => x.EventType)
            .IsInEnum().WithMessage("EventType must be Conference, Workshop, or Concert.");
    }
}
