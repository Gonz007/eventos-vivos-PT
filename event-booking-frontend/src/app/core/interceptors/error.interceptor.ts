import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let message = 'An unexpected error occurred.';

      if (error.error && typeof error.error === 'object') {
        // ProblemDetails format
        if (error.error.detail) {
          message = error.error.detail;
        } else if (error.error.title) {
          message = error.error.title;
        }

        // Validation errors
        if (error.error.errors) {
          const errors = error.error.errors as Record<string, string[]>;
          const messages = Object.values(errors).flat();
          message = messages.join(' | ');
        }
      } else if (error.message) {
        message = error.message;
      }

      switch (error.status) {
        case 400:
          snackBar.open(`Validation error: ${message}`, 'Close', { duration: 5000, panelClass: 'snack-error' });
          break;
        case 404:
          snackBar.open(`Not found: ${message}`, 'Close', { duration: 4000, panelClass: 'snack-error' });
          break;
        case 422:
          snackBar.open(`Business rule: ${message}`, 'Close', { duration: 5000, panelClass: 'snack-warn' });
          break;
        case 0:
          snackBar.open('Cannot connect to the server. Make sure the API is running.', 'Close', { duration: 6000, panelClass: 'snack-error' });
          break;
        default:
          snackBar.open(`Error ${error.status}: ${message}`, 'Close', { duration: 5000, panelClass: 'snack-error' });
      }

      return throwError(() => error);
    })
  );
};
