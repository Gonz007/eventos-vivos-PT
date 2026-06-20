using EventBooking.Domain.Enums;

namespace EventBooking.Domain.Entities;

public class Event
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int VenueId { get; set; }
    public Venue Venue { get; set; } = null!;
    public int MaxCapacity { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TicketPrice { get; set; }
    public EventType EventType { get; set; }
    public EventStatus Status { get; set; } = EventStatus.Active;

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
