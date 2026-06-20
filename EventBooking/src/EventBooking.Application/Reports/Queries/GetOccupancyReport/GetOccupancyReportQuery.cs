using EventBooking.Application.DTOs;
using MediatR;

namespace EventBooking.Application.Reports.Queries.GetOccupancyReport;

public record GetOccupancyReportQuery(int EventId) : IRequest<OccupancyReportDto>;
