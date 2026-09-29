import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap } from 'rxjs';
import { AccountsApi } from '../../../core/api/accounts-api';
import { ApiError } from '../../../core/models/api-error';
import * as AccountsActions from './accounts.actions';

@Injectable()
export class AccountsEffects {
  private readonly actions = inject(Actions);
  private readonly accountsApi = inject(AccountsApi);

  readonly load = createEffect(() =>
    this.actions.pipe(
      ofType(AccountsActions.loadAccounts),
      switchMap(() =>
        this.accountsApi.getAll().pipe(
          map((accounts) => AccountsActions.loadAccountsSuccess({ accounts })),
          catchError((error: ApiError) => of(AccountsActions.loadAccountsFailure({ error: error.message }))),
        ),
      ),
    ),
  );
}
