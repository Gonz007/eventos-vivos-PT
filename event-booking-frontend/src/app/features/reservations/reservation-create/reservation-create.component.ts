import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ReservationsService } from '../../../core/services/reservations.service';
import { Reservation } from '../../../shared/models/reservation.model';

@Component({
  selector: 'app-reservation-create',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, RouterLink,
    MatCardModule, MatFormFieldModule, MatInputModule,
    MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatDividerModule
  ],
  templateUrl: './reservation-create.component.html',
  styleUrls: ['./reservation-create.component.scss']
})
export class ReservationCreateComponent implements OnInit {
  private readonly reservationsService = inject(ReservationsService);
  private readonly route = inject(ActivatedRoute);
  private readonly snackBar = inject(MatSnackBar);
  private readonly fb = inject(FormBuilder);

  loading = false;
  created = signal<Reservation | null>(null);

  form = this.fb.group({
    eventId:   [null as number | null, [Validators.required, Validators.min(1)]],
    buyerName: ['', [Validators.required, Validators.minLength(2)]],
    buyerEmail:['', [Validators.required, Validators.email]],
    quantity:  [1,  [Validators.required, Validators.min(1), Validators.max(100)]]
  });

  ngOnInit(): void {
    const eventId = this.route.snapshot.queryParamMap.get('eventId');
    if (eventId) {
      this.form.patchValue({ eventId: Number(eventId) });
    }
  }

  get f() { return this.form.controls; }

  getError(control: string): string {
    const c = this.form.get(control);
    if (!c || !c.errors || !c.touched) return '';
    if (c.errors['required'])   return 'This field is required.';
    if (c.errors['email'])      return 'Enter a valid email address.';
    if (c.errors['minlength'])  return `Minimum ${c.errors['minlength'].requiredLength} characters.`;
    if (c.errors['min'])        return `Minimum value is ${c.errors['min'].min}.`;
    if (c.errors['max'])        return `Maximum value is ${c.errors['max'].max}.`;
    return 'Invalid value.';
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading = true;
    const v = this.form.value;

    this.reservationsService.createReservation({
      eventId:    v.eventId!,
      quantity:   v.quantity!,
      buyerName:  v.buyerName!,
      buyerEmail: v.buyerEmail!
    }).subscribe({
      next: (reservation) => {
        this.loading = false;
        this.created.set(reservation);
        this.snackBar.open('Reservation created successfully!', 'Close', {
          duration: 4000, panelClass: 'snack-success'
        });
      },
      error: () => this.loading = false
    });
  }

  reset(): void {
    this.created.set(null);
    this.form.reset({ quantity: 1 });
  }

  getStatusClass(status: string): string {
    return status?.toLowerCase().replace(/\s/g, '') ?? '';
  }
}
