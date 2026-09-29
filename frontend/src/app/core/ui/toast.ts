import { Injectable, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

const SuccessDurationMs = 6000;
const ErrorDurationMs = 9000;

@Injectable({ providedIn: 'root' })
export class Toast {
  private readonly snackBar = inject(MatSnackBar);

  showSuccess(message: string, actionLabel: string, action: () => void): void {
    this.snackBar
      .open(message, actionLabel, {
        duration: SuccessDurationMs,
        panelClass: ['toast', 'toast--success'],
        horizontalPosition: 'right',
        verticalPosition: 'bottom',
      })
      .onAction()
      .subscribe(() => action());
  }

  showError(message: string): void {
    this.snackBar.open(message, 'Fechar', {
      duration: ErrorDurationMs,
      panelClass: ['toast', 'toast--error'],
      horizontalPosition: 'right',
      verticalPosition: 'bottom',
    });
  }
}
