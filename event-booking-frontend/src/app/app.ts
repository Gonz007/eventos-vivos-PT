import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatToolbarModule, MatButtonModule, MatIconModule],
  template: `
    <mat-toolbar color="primary" class="app-toolbar">
      <mat-icon>event</mat-icon>
      <span class="toolbar-title" routerLink="/events" style="cursor:pointer; margin-left:8px;">
        EventosVivos
      </span>
      <span class="spacer"></span>
      <a mat-button routerLink="/events" routerLinkActive="active-link">
        <mat-icon>list</mat-icon> Events
      </a>
      <a mat-button routerLink="/events/create" routerLinkActive="active-link">
        <mat-icon>add_circle</mat-icon> Create Event
      </a>
      <a mat-button routerLink="/reservations/create" routerLinkActive="active-link">
        <mat-icon>bookmark_add</mat-icon> Reserve
      </a>
    </mat-toolbar>

    <main class="app-content">
      <router-outlet />
    </main>
  `,
  styles: [`
    .app-toolbar { position: sticky; top: 0; z-index: 100; }
    .toolbar-title { font-size: 1.2rem; font-weight: 600; letter-spacing: 0.5px; }
    .spacer { flex: 1 1 auto; }
    .app-content { padding: 24px; max-width: 1200px; margin: 0 auto; }
    .active-link { background: rgba(255,255,255,0.15) !important; border-radius: 4px; }
    a mat-icon { margin-right: 4px; font-size: 18px; vertical-align: middle; }
  `]
})
export class App {
  title = 'event-booking-frontend';
}
