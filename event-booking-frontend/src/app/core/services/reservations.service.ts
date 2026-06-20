import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Reservation, CreateReservationPayload } from '../../shared/models/reservation.model';

@Injectable({ providedIn: 'root' })
export class ReservationsService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/reservations`;

  createReservation(payload: CreateReservationPayload): Observable<Reservation> {
    return this.http.post<Reservation>(this.base, payload);
  }

  getReservation(id: number): Observable<Reservation> {
    return this.http.get<Reservation>(`${this.base}/${id}`);
  }

  confirmReservation(id: number): Observable<Reservation> {
    return this.http.post<Reservation>(`${this.base}/${id}/confirm`, {});
  }

  cancelReservation(id: number): Observable<Reservation> {
    return this.http.post<Reservation>(`${this.base}/${id}/cancel`, {});
  }
}
