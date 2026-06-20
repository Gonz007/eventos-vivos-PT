using EventBooking.Domain.Enums;

namespace EventBooking.Domain.Entities;

public class Reservation
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event Event { get; set; } = null!;
    public int Quantity { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public string BuyerEmail { get; set; } = string.Empty;
    public ReservationStatus Status { get; set; } = ReservationStatus.PendingPayment;
    public string? ReservationCode { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CancelledAt { get; set; }
    public bool IsLostSale { get; set; } = false;
}
