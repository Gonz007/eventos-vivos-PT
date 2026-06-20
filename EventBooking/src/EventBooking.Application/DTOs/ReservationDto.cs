namespace EventBooking.Application.DTOs;

public record ReservationDto(
    int Id,
    int EventId,
    string EventTitle,
    int Quantity,
    string BuyerName,
    string BuyerEmail,
    string Status,
    string? ReservationCode,
    DateTime CreatedAt,
    DateTime? CancelledAt,
    bool IsLostSale
);
