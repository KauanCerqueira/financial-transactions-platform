import { ProcessedTransaction } from '../../../core/models/processed-transaction';
import { ProcessTransactionCommand } from '../../../core/models/transaction';
import * as TransactionsActions from './transactions.actions';
import { transactionsReducer } from './transactions.reducer';

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

describe('transactionsReducer', () => {
  it('marks submitting on submit', () => {
    const state = transactionsReducer(undefined, TransactionsActions.submitTransaction({ command }));

    expect(state.status).toBe('submitting');
    expect(state.result).toBeNull();
    expect(state.error).toBeNull();
  });

  it('stores the result on success', () => {
    const state = transactionsReducer(undefined, TransactionsActions.submitTransactionSuccess({ result: processed }));

    expect(state.status).toBe('success');
    expect(state.result).toEqual(processed);
  });

  it('stores the business error on failure', () => {
    const state = transactionsReducer(
      undefined,
      TransactionsActions.submitTransactionFailure({ message: 'Saldo insuficiente', code: 'INSUFFICIENT_FUNDS' }),
    );

    expect(state.status).toBe('error');
    expect(state.error).toEqual({ message: 'Saldo insuficiente', code: 'INSUFFICIENT_FUNDS' });
  });

  it('clears back to idle', () => {
    const failed = transactionsReducer(
      undefined,
      TransactionsActions.submitTransactionFailure({ message: 'x', code: 'y' }),
    );

    const state = transactionsReducer(failed, TransactionsActions.clearTransactionResult());

    expect(state.status).toBe('idle');
    expect(state.error).toBeNull();
  });
});
