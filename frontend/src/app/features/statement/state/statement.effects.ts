import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap } from 'rxjs';
import { AccountsApi } from '../../../core/api/accounts-api';
import { ApiError } from '../../../core/models/api-error';
import * as StatementActions from './statement.actions';

@Injectable()
export class StatementEffects {
  private readonly actions = inject(Actions);
  private readonly accountsApi = inject(AccountsApi);

  readonly load = createEffect(() =>
    this.actions.pipe(
      ofType(StatementActions.loadStatement),
      switchMap(({ accountId, page, pageSize }) =>
        this.accountsApi.getStatement(accountId, page, pageSize).pipe(
          map((result) => StatementActions.loadStatementSuccess({ result })),
          catchError((error: ApiError) => of(StatementActions.loadStatementFailure({ error: error.message }))),
        ),
      ),
    ),
  );
}
