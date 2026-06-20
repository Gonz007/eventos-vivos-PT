import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { EventsService } from '../../../core/services/events.service';
import { Event, EventFilters } from '../../../shared/models/event.model';

@Component({
  selector: 'app-event-list',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, RouterLink,
    MatTableModule, MatCardModule, MatButtonModule, MatIconModule,
    MatFormFieldModule, MatInputModule, MatSelectModule,
    MatProgressSpinnerModule, MatTooltipModule
  ],
  templateUrl: './event-list.component.html',
  styleUrls: ['./event-list.component.scss']
})
export class EventListComponent implements OnInit {
  private readonly eventsService = inject(EventsService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  events = signal<Event[]>([]);
  loading = signal(false);

  displayedColumns = ['title', 'venueName', 'maxCapacity', 'eventType', 'status', 'startDate', 'ticketPrice', 'actions'];

  filterForm = this.fb.group({
    search: [''],
    type: [null as number | null],
    status: [null as number | null]
  });

  eventTypes = [
    { label: 'Conference', value: 1 },
    { label: 'Workshop',   value: 2 },
    { label: 'Concert',    value: 3 }
  ];

  eventStatuses = [
    { label: 'Active',    value: 1 },
    { label: 'Cancelled', value: 2 },
    { label: 'Completed', value: 3 }
  ];

  ngOnInit(): void {
    this.loadEvents();

    this.filterForm.valueChanges
      .pipe(debounceTime(400), distinctUntilChanged())
      .subscribe(() => this.loadEvents());
  }

  loadEvents(): void {
    this.loading.set(true);
    const { search, type, status } = this.filterForm.value;
    const filters: EventFilters = {};
    if (search?.trim()) filters.search = search.trim();
    if (type != null)   filters.type = type;
    if (status != null) filters.status = status;

    this.eventsService.getEvents(filters).subscribe({
      next: data => { this.events.set(data); this.loading.set(false); },
      error: ()   => this.loading.set(false)
    });
  }

  clearFilters(): void {
    this.filterForm.reset();
  }

  viewReport(id: number): void {
    this.router.navigate(['/events', id, 'report']);
  }

  createReservation(eventId: number): void {
    this.router.navigate(['/reservations/create'], { queryParams: { eventId } });
  }

  getStatusClass(status: string): string {
    return status?.toLowerCase().replace(/\s/g, '') ?? '';
  }
}
