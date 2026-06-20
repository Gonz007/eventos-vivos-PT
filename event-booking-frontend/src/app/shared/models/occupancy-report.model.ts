export interface OccupancyReport {
  eventId: number;
  eventTitle: string;
  soldTickets: number;
  availableTickets: number;
  occupancyPercentage: number;
  totalRevenue: number;
  status: string;
}
