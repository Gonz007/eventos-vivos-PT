export interface Event {
  id: number;
  title: string;
  description: string;
  venueId: number;
  venueName: string;
  maxCapacity: number;
  startDate: string;
  endDate: string;
  ticketPrice: number;
  eventType: string;
  status: string;
}

export interface CreateEventPayload {
  title: string;
  description: string;
  venueId: number;
  maxCapacity: number;
  startDate: string;
  endDate: string;
  ticketPrice: number;
  eventType: number;
}

export interface EventFilters {
  type?: number;
  venueId?: number;
  status?: number;
  search?: string;
  startDateFrom?: string;
  startDateTo?: string;
}
