import { TestBed } from '@angular/core/testing';
import { provideMockActions } from '@ngrx/effects/testing';
import { Observable, ReplaySubject, firstValueFrom, of, throwError } from 'rxjs';
import { TransactionsApi } from '../../../core/api/transactions-api';
import { ApiError } from '../../../core/models/api-error';
import { ProcessedTransaction } from '../../../core/models/processed-transaction';
import { ProcessTransactionCommand } from '../../../core/models/transaction';
import * as TransactionsActions from './transactions.actions';
import { TransactionsEffects } from './transactions.effects';

const command: ProcessTransactionCommand = {
  eventId: 'e1',
  accountId: 'a1',
  type: 'CREDIT',
  amount: 100,
  occurredAt: '2026-01-30T10:00:00Z',
};

const processed: ProcessedTransaction = {
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
  let api: { process: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    actions$ = new ReplaySubject(1);
    api = { process: vi.fn() };

    TestBed.configureTestingModule({
      providers: [
        TransactionsEffects,
        provideMockActions(() => actions$),
        { provide: TransactionsApi, useValue: api },
      ],
    });

    effects = TestBed.inject(TransactionsEffects);
  });

  it('dispatches success when the API confirms the transaction', async () => {
    api.process.mockReturnValue(of(processed));

    const emitted = firstValueFrom(effects.submit);
    (actions$ as ReplaySubject<unknown>).next(TransactionsActions.submitTransaction({ command }));

    expect(await emitted).toEqual(TransactionsActions.submitTransactionSuccess({ result: processed }));
    expect(api.process).toHaveBeenCalledWith(command);
  });

  it('dispatches failure with the business message when the API rejects', async () => {
    api.process.mockReturnValue(throwError(() => new ApiError('INSUFFICIENT_FUNDS', 'Saldo insuficiente', 422)));

    const emitted = firstValueFrom(effects.submit);
    (actions$ as ReplaySubject<unknown>).next(TransactionsActions.submitTransaction({ command }));

    expect(await emitted).toEqual(
      TransactionsActions.submitTransactionFailure({ message: 'Saldo insuficiente', code: 'INSUFFICIENT_FUNDS' }),
    );
  });
});
