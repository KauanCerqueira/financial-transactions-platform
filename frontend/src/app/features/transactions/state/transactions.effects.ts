import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, map, of, switchMap } from 'rxjs';
import { TransactionsApi } from '../../../core/api/transactions-api';
import { ApiError } from '../../../core/models/api-error';
import * as TransactionsActions from './transactions.actions';

@Injectable()
export class TransactionsEffects {
  private readonly actions = inject(Actions);
  private readonly transactionsApi = inject(TransactionsApi);

  readonly submit = createEffect(() =>
    this.actions.pipe(
      ofType(TransactionsActions.submitTransaction),
      switchMap(({ command }) =>
        this.transactionsApi.process(command).pipe(
          map((result) => TransactionsActions.submitTransactionSuccess({ result })),
          catchError((error: ApiError) =>
            of(TransactionsActions.submitTransactionFailure({ message: error.message, code: error.code })),
          ),
        ),
      ),
    ),
  );
}
