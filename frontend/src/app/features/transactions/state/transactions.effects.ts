import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { catchError, defer, map, mergeMap, of } from 'rxjs';
import { TransactionsApi } from '../../../core/api/transactions-api';
import { ApiError } from '../../../core/models/api-error';
import { forgetPendingTransaction, rememberPendingTransaction } from '../pending-transaction';
import * as TransactionsActions from './transactions.actions';

const FirstClientErrorStatus = 400;
const FirstServerErrorStatus = 500;

@Injectable()
export class TransactionsEffects {
  private readonly actions = inject(Actions);
  private readonly transactionsApi = inject(TransactionsApi);

  readonly submit = createEffect(() =>
    this.actions.pipe(
      ofType(TransactionsActions.submitTransaction),
      mergeMap(({ command }) =>
        defer(() => {
          rememberPendingTransaction(command);
          return this.transactionsApi.process(command);
        }).pipe(
          map((result) => {
            forgetPendingTransaction();
            return TransactionsActions.transactionResolved({ result });
          }),
          catchError((error: ApiError) => {
            if (isClientError(error)) {
              forgetPendingTransaction();
            }

            return of(TransactionsActions.submitTransactionFailure({ message: error.message, code: error.code }));
          }),
        ),
      ),
    ),
  );
}

// Erros de cliente (regra de negócio ou validação) não se resolvem ao reenviar; falhas de rede ou do servidor, sim.
function isClientError(error: ApiError): boolean {
  return error.status >= FirstClientErrorStatus && error.status < FirstServerErrorStatus;
}
