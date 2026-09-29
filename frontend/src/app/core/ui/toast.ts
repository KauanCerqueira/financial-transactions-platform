import { Injectable, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

const NoticeDurationMs = 6000;
const ErrorDurationMs = 9000;

@Injectable({ providedIn: 'root' })
export class Toast {
  private readonly snackBar = inject(MatSnackBar);

  showSuccess(message: string, actionLabel: string, action: () => void): void {
    this.open(message, 'toast--success', NoticeDurationMs, actionLabel, action);
  }

  showInfo(message: string, actionLabel: string, action: () => void): void {
    this.open(message, 'toast--info', NoticeDurationMs, actionLabel, action);
  }

  showError(message: string): void {
    this.open(message, 'toast--error', ErrorDurationMs, 'Fechar');
  }

  private open(message: string, modifier: string, duration: number, actionLabel: string, action?: () => void): void {
    const reference = this.snackBar.open(message, actionLabel, {
      duration,
      panelClass: ['toast', modifier],
      horizontalPosition: 'right',
      verticalPosition: 'bottom',
    });

    if (action) {
      reference.onAction().subscribe(() => action());
    }
  }
}
