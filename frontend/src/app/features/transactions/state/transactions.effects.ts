import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Action } from '@ngrx/store';
import {
  EMPTY,
  Observable,
  catchError,
  concat,
  concatWith,
  defer,
  map,
  mergeMap,
  of,
  switchMap,
  take,
  takeWhile,
  tap,
  timer,
} from 'rxjs';
import { TransactionsApi } from '../../../core/api/transactions-api';
import { ApiError } from '../../../core/models/api-error';
import { forgetPendingTransaction, rememberPendingTransaction } from '../pending-transaction';
import * as TransactionsActions from './transactions.actions';

const PollIntervalMs = 700;
const MaxPolls = 40;

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
          return this.transactionsApi.enqueue(command);
        }).pipe(
          mergeMap((accepted) =>
            concat(
              of(TransactionsActions.transactionAccepted({ result: accepted })).pipe(
                tap(() => {
                  if (accepted.status !== 'PENDING') {
                    forgetPendingTransaction();
                  }
                }),
              ),
              accepted.status === 'PENDING' ? this.pollStatus(accepted.eventId) : EMPTY,
            ),
          ),
          catchError((error: ApiError) =>
            of(TransactionsActions.submitTransactionFailure({ message: error.message, code: error.code })),
          ),
        ),
      ),
    ),
  );

  private pollStatus(eventId: string): Observable<Action> {
    return timer(PollIntervalMs, PollIntervalMs).pipe(
      take(MaxPolls),
      switchMap(() => this.transactionsApi.getStatus(eventId)),
      takeWhile((result) => result.status === 'PENDING', true),
      map((result) => {
        if (result.status !== 'PENDING') {
          forgetPendingTransaction();
        }

        return TransactionsActions.transactionResolved({ result });
      }),
      concatWith(of(TransactionsActions.transactionPollingTimedOut({ eventId }))),
    );
  }
}
