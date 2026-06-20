import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'events', pathMatch: 'full' },
  {
    path: 'events',
    loadComponent: () =>
      import('./features/events/event-list/event-list.component').then(m => m.EventListComponent)
  },
  {
    path: 'events/create',
    loadComponent: () =>
      import('./features/events/event-create/event-create.component').then(m => m.EventCreateComponent)
  },
  {
    path: 'events/:id/report',
    loadComponent: () =>
      import('./features/events/event-report/event-report.component').then(m => m.EventReportComponent)
  },
  {
    path: 'reservations/create',
    loadComponent: () =>
      import('./features/reservations/reservation-create/reservation-create.component').then(m => m.ReservationCreateComponent)
  },
  {
    path: 'reservations/:id',
    loadComponent: () =>
      import('./features/reservations/reservation-detail/reservation-detail.component').then(m => m.ReservationDetailComponent)
  },
  { path: '**', redirectTo: 'events' }
];
