import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { tap } from 'rxjs';
import { Toast } from '../../../core/ui/toast';
import * as AccountsActions from './accounts.actions';

@Injectable()
export class AccountsNotifications {
  private readonly actions = inject(Actions);
  private readonly toast = inject(Toast);
  private readonly router = inject(Router);

  readonly created = createEffect(
    () =>
      this.actions.pipe(
        ofType(AccountsActions.createAccountSuccess),
        tap(({ account }) => {
          const openStatement = () => void this.router.navigate(['/accounts', account.id, 'statement']);

          this.toast.showSuccess(`Conta de ${account.holderName} criada.`, 'Ver extrato', openStatement);
        }),
      ),
    { dispatch: false },
  );

  readonly failed = createEffect(
    () =>
      this.actions.pipe(
        ofType(AccountsActions.createAccountFailure),
        tap(({ message }) => this.toast.showError(message)),
      ),
    { dispatch: false },
  );
}
