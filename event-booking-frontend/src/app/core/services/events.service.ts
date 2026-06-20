import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Event, CreateEventPayload, EventFilters } from '../../shared/models/event.model';
import { OccupancyReport } from '../../shared/models/occupancy-report.model';

@Injectable({ providedIn: 'root' })
export class EventsService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/events`;

  getEvents(filters?: EventFilters): Observable<Event[]> {
    let params = new HttpParams();
    if (filters) {
      if (filters.type != null)          params = params.set('type', filters.type);
      if (filters.venueId != null)       params = params.set('venueId', filters.venueId);
      if (filters.status != null)        params = params.set('status', filters.status);
      if (filters.search)                params = params.set('search', filters.search);
      if (filters.startDateFrom)         params = params.set('startDateFrom', filters.startDateFrom);
      if (filters.startDateTo)           params = params.set('startDateTo', filters.startDateTo);
    }
    return this.http.get<Event[]>(this.base, { params });
  }

  getEvent(id: number): Observable<Event> {
    return this.http.get<Event>(`${this.base}/${id}`);
  }

  createEvent(payload: CreateEventPayload): Observable<Event> {
    return this.http.post<Event>(this.base, payload);
  }

  getReport(id: number): Observable<OccupancyReport> {
    return this.http.get<OccupancyReport>(`${this.base}/${id}/report`);
  }
}
