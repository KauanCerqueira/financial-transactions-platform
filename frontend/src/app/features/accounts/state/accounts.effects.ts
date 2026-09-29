import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, forkJoin, map, of, switchMap } from 'rxjs';
import { AccountsApi } from '../../../core/api/accounts-api';
import { ApiError } from '../../../core/models/api-error';
import * as AccountsActions from './accounts.actions';

@Injectable()
export class AccountsEffects {
  private readonly actions = inject(Actions);
  private readonly accountsApi = inject(AccountsApi);

  readonly load = createEffect(() =>
    this.actions.pipe(
      ofType(AccountsActions.loadAccounts, AccountsActions.refreshAccounts),
      switchMap(() =>
        forkJoin({
          accounts: this.accountsApi.getAll(),
          summary: this.accountsApi.getSummary(),
        }).pipe(
          map(({ accounts, summary }) => AccountsActions.loadAccountsSuccess({ accounts, summary })),
          catchError((error: ApiError) => of(AccountsActions.loadAccountsFailure({ error: error.message }))),
        ),
      ),
    ),
  );

  readonly create = createEffect(() =>
    this.actions.pipe(
      ofType(AccountsActions.createAccount),
      switchMap(({ holderName, initialBalance }) =>
        this.accountsApi.createAccount({ holderName, initialBalance }).pipe(
          map((account) => AccountsActions.createAccountSuccess({ account })),
          catchError((error: ApiError) =>
            of(AccountsActions.createAccountFailure({ message: error.message, code: error.code })),
          ),
        ),
      ),
    ),
  );

  // o resumo (saldo consolidado e total de lançamentos) muda ao criar uma conta
  readonly refreshAfterCreate = createEffect(() =>
    this.actions.pipe(
      ofType(AccountsActions.createAccountSuccess),
      map(() => AccountsActions.refreshAccounts()),
    ),
  );
}
