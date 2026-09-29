import { TestBed } from '@angular/core/testing';
import { provideMockActions } from '@ngrx/effects/testing';
import { Observable, ReplaySubject, firstValueFrom, of, throwError } from 'rxjs';
import { TransactionsApi } from '../../../core/api/transactions-api';
import { ApiError } from '../../../core/models/api-error';
import { ProcessTransactionCommand } from '../../../core/models/transaction';
import { TransactionAccepted } from '../../../core/models/transaction-event';
import * as TransactionsActions from './transactions.actions';
import { TransactionsEffects } from './transactions.effects';

const command: ProcessTransactionCommand = {
  eventId: 'e1',
  accountId: 'a1',
  type: 'CREDIT',
  amount: 100,
  occurredAt: '2026-01-30T10:00:00Z',
};

const processed: TransactionAccepted = {
  eventId: 'e1',
  status: 'PROCESSED',
  rejectionCode: null,
  transaction: {
    id: 't1',
    eventId: 'e1',
    accountId: 'a1',
    type: 'CREDIT',
    amount: 100,
    occurredAt: '2026-01-30T10:00:00Z',
    balanceAfter: 200,
    recordedAt: '2026-01-30T10:00:00Z',
  },
  alreadyProcessed: false,
};

describe('TransactionsEffects', () => {
  let actions$: Observable<unknown>;
  let effects: TransactionsEffects;
  let api: { process: jasmine.Spy };

  beforeEach(() => {
    actions$ = new ReplaySubject(1);
    api = { process: jasmine.createSpy('process') };

    TestBed.configureTestingModule({
      providers: [
        TransactionsEffects,
        provideMockActions(() => actions$),
        { provide: TransactionsApi, useValue: api },
      ],
    });

    effects = TestBed.inject(TransactionsEffects);
  });

  it('dispatches resolved when the API processes the transaction', async () => {
    api.process.and.returnValue(of(processed));

    const emitted = firstValueFrom(effects.submit);
    (actions$ as ReplaySubject<unknown>).next(TransactionsActions.submitTransaction({ command }));

    expect(await emitted).toEqual(TransactionsActions.transactionResolved({ result: processed }));
    expect(api.process).toHaveBeenCalledWith(command);
  });

  it('dispatches failure with the business message when the API rejects', async () => {
    api.process.and.returnValue(throwError(() => new ApiError('INSUFFICIENT_FUNDS', 'Saldo insuficiente', 422)));

    const emitted = firstValueFrom(effects.submit);
    (actions$ as ReplaySubject<unknown>).next(TransactionsActions.submitTransaction({ command }));

    expect(await emitted).toEqual(
      TransactionsActions.submitTransactionFailure({ message: 'Saldo insuficiente', code: 'INSUFFICIENT_FUNDS' }),
    );
  });
});
