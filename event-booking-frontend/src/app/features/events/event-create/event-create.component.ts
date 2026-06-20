import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { EventsService } from '../../../core/services/events.service';

@Component({
  selector: 'app-event-create',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, RouterLink,
    MatCardModule, MatFormFieldModule, MatInputModule, MatSelectModule,
    MatButtonModule, MatIconModule, MatDatepickerModule, MatNativeDateModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './event-create.component.html',
  styleUrls: ['./event-create.component.scss']
})
export class EventCreateComponent {
  private readonly eventsService = inject(EventsService);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);
  private readonly fb = inject(FormBuilder);

  loading = false;
  minDate = new Date();

  venues = [
    { id: 1, name: 'Auditorio Central — Bogotá (cap. 200)' },
    { id: 2, name: 'Sala Norte — Bogotá (cap. 50)' },
    { id: 3, name: 'Arena Sur — Medellín (cap. 500)' }
  ];

  eventTypes = [
    { label: 'Conference', value: 1 },
    { label: 'Workshop',   value: 2 },
    { label: 'Concert',    value: 3 }
  ];

  form = this.fb.group({
    title:       ['', [Validators.required, Validators.minLength(5), Validators.maxLength(100)]],
    description: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(500)]],
    venueId:     [null as number | null, Validators.required],
    maxCapacity: [null as number | null, [Validators.required, Validators.min(1)]],
    startDate:   [null as Date | null,   Validators.required],
    endDate:     [null as Date | null,   Validators.required],
    ticketPrice: [null as number | null, [Validators.required, Validators.min(0.01)]],
    eventType:   [null as number | null, Validators.required]
  }, { validators: this.endAfterStart });

  private endAfterStart(group: AbstractControl) {
    const start = group.get('startDate')?.value as Date | null;
    const end   = group.get('endDate')?.value as Date | null;
    if (start && end && end <= start) {
      group.get('endDate')?.setErrors({ endBeforeStart: true });
      return { endBeforeStart: true };
    }
    return null;
  }

  get f() { return this.form.controls; }

  getError(control: string): string {
    const c = this.form.get(control);
    if (!c || !c.errors || !c.touched) return '';
    if (c.errors['required'])      return 'This field is required.';
    if (c.errors['minlength'])     return `Minimum ${c.errors['minlength'].requiredLength} characters.`;
    if (c.errors['maxlength'])     return `Maximum ${c.errors['maxlength'].requiredLength} characters.`;
    if (c.errors['min'])           return `Value must be greater than ${c.errors['min'].min}.`;
    if (c.errors['endBeforeStart']) return 'End date must be after start date.';
    return 'Invalid value.';
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading = true;
    const v = this.form.value;

    const payload = {
      title:       v.title!,
      description: v.description!,
      venueId:     v.venueId!,
      maxCapacity: v.maxCapacity!,
      startDate:   (v.startDate as Date).toISOString(),
      endDate:     (v.endDate as Date).toISOString(),
      ticketPrice: v.ticketPrice!,
      eventType:   v.eventType!
    };

    this.eventsService.createEvent(payload).subscribe({
      next: (event) => {
        this.loading = false;
        this.snackBar.open(`Event "${event.title}" created successfully!`, 'Close', {
          duration: 4000, panelClass: 'snack-success'
        });
        this.router.navigate(['/events']);
      },
      error: () => this.loading = false
    });
  }
}
