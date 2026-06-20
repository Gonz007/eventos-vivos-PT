import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialogModule } from '@angular/material/dialog';
import { ReservationsService } from '../../../core/services/reservations.service';
import { Reservation } from '../../../shared/models/reservation.model';

@Component({
  selector: 'app-reservation-detail',
  standalone: true,
  imports: [
    CommonModule, RouterLink,
    MatCardModule, MatButtonModule, MatIconModule,
    MatProgressSpinnerModule, MatDividerModule, MatDialogModule
  ],
  templateUrl: './reservation-detail.component.html',
  styleUrls: ['./reservation-detail.component.scss']
})
export class ReservationDetailComponent implements OnInit {
  private readonly reservationsService = inject(ReservationsService);
  private readonly route = inject(ActivatedRoute);
  private readonly snackBar = inject(MatSnackBar);

  reservation = signal<Reservation | null>(null);
  loading = signal(true);
  actionLoading = signal(false);
  reservationId = 0;

  ngOnInit(): void {
    this.reservationId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadReservation();
  }

  loadReservation(): void {
    this.loading.set(true);
    this.reservationsService.getReservation(this.reservationId).subscribe({
      next: data => { this.reservation.set(data); this.loading.set(false); },
      error: ()   => this.loading.set(false)
    });
  }

  confirmPayment(): void {
    if (this.actionLoading()) return;
    this.actionLoading.set(true);
    this.reservationsService.confirmReservation(this.reservationId).subscribe({
      next: (updated) => {
        this.reservation.set(updated);
        this.actionLoading.set(false);
        this.snackBar.open('Payment confirmed! Reservation is now confirmed.', 'Close', {
          duration: 4000, panelClass: 'snack-success'
        });
      },
      error: () => this.actionLoading.set(false)
    });
  }

  cancelReservation(): void {
    if (this.actionLoading()) return;
    this.actionLoading.set(true);
    this.reservationsService.cancelReservation(this.reservationId).subscribe({
      next: (updated) => {
        this.reservation.set(updated);
        this.actionLoading.set(false);
        const msg = updated.isLostSale
          ? 'Reservation cancelled (marked as lost sale — within 48h of event).'
          : 'Reservation cancelled successfully.';
        this.snackBar.open(msg, 'Close', { duration: 5000, panelClass: 'snack-warn' });
      },
      error: () => this.actionLoading.set(false)
    });
  }

  getStatusClass(status: string): string {
    return status?.toLowerCase().replace(/\s/g, '') ?? '';
  }

  canConfirm(): boolean {
    const s = this.reservation()?.status;
    return s === 'PendingPayment';
  }

  canCancel(): boolean {
    const s = this.reservation()?.status;
    return s === 'PendingPayment' || s === 'Confirmed';
  }
}
