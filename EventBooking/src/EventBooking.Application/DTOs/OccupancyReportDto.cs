namespace EventBooking.Application.DTOs;

public record OccupancyReportDto(
    int EventId,
    string EventTitle,
    int SoldTickets,
    int AvailableTickets,
    decimal OccupancyPercentage,
    decimal TotalRevenue,
    string Status
);
