import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { EventsService } from '../../../core/services/events.service';
import { OccupancyReport } from '../../../shared/models/occupancy-report.model';

@Component({
  selector: 'app-event-report',
  standalone: true,
  imports: [
    CommonModule, RouterLink,
    MatCardModule, MatButtonModule, MatIconModule,
    MatProgressSpinnerModule, MatDividerModule
  ],
  templateUrl: './event-report.component.html',
  styleUrls: ['./event-report.component.scss']
})
export class EventReportComponent implements OnInit {
  private readonly eventsService = inject(EventsService);
  private readonly route = inject(ActivatedRoute);

  report = signal<OccupancyReport | null>(null);
  loading = signal(true);
  eventId = 0;

  ngOnInit(): void {
    this.eventId = Number(this.route.snapshot.paramMap.get('id'));
    this.loadReport();
  }

  loadReport(): void {
    this.loading.set(true);
    this.eventsService.getReport(this.eventId).subscribe({
      next: data => { this.report.set(data); this.loading.set(false); },
      error: ()   => this.loading.set(false)
    });
  }

  getStatusClass(status: string): string {
    return status?.toLowerCase().replace(/\s/g, '') ?? '';
  }

  getOccupancyColor(pct: number): string {
    if (pct >= 90) return '#c62828';
    if (pct >= 70) return '#e65100';
    if (pct >= 40) return '#2e7d32';
    return '#1565c0';
  }
}
