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

const pending: TransactionAccepted = { eventId: 'e1', status: 'PENDING', rejectionCode: null, transaction: null };

describe('TransactionsEffects', () => {
  let actions$: Observable<unknown>;
  let effects: TransactionsEffects;
  let api: { enqueue: ReturnType<typeof vi.fn>; getStatus: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    actions$ = new ReplaySubject(1);
    api = { enqueue: vi.fn(), getStatus: vi.fn() };

    TestBed.configureTestingModule({
      providers: [
        TransactionsEffects,
        provideMockActions(() => actions$),
        { provide: TransactionsApi, useValue: api },
      ],
    });

    effects = TestBed.inject(TransactionsEffects);
  });

  it('dispatches accepted when the API enqueues the event', async () => {
    api.enqueue.mockReturnValue(of(pending));

    const emitted = firstValueFrom(effects.submit);
    (actions$ as ReplaySubject<unknown>).next(TransactionsActions.submitTransaction({ command }));

    expect(await emitted).toEqual(TransactionsActions.transactionAccepted({ result: pending }));
    expect(api.enqueue).toHaveBeenCalledWith(command);
  });

  it('does not poll when the event is already resolved', async () => {
    api.enqueue.mockReturnValue(of({ ...pending, status: 'PROCESSED' as const }));

    const emitted = firstValueFrom(effects.submit);
    (actions$ as ReplaySubject<unknown>).next(TransactionsActions.submitTransaction({ command }));
    await emitted;

    expect(api.getStatus).not.toHaveBeenCalled();
  });

  it('dispatches failure when the API rejects the enqueue', async () => {
    api.enqueue.mockReturnValue(throwError(() => new ApiError('NETWORK_ERROR', 'Sem conexão', 0)));

    const emitted = firstValueFrom(effects.submit);
    (actions$ as ReplaySubject<unknown>).next(TransactionsActions.submitTransaction({ command }));

    expect(await emitted).toEqual(
      TransactionsActions.submitTransactionFailure({ message: 'Sem conexão', code: 'NETWORK_ERROR' }),
    );
  });
});
