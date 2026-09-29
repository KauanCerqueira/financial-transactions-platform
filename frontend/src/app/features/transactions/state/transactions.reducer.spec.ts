import { TransactionAccepted } from '../../../core/models/transaction-event';
import * as TransactionsActions from './transactions.actions';
import { transactionsReducer } from './transactions.reducer';

const command = {
  eventId: 'e1',
  accountId: 'a1',
  type: 'CREDIT' as const,
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

const rejected: TransactionAccepted = {
  eventId: 'e1',
  status: 'REJECTED',
  rejectionCode: 'INSUFFICIENT_FUNDS',
  transaction: null,
  alreadyProcessed: false,
};

describe('transactionsReducer', () => {
  it('stores the command and marks submitting on submit', () => {
    const state = transactionsReducer(undefined, TransactionsActions.submitTransaction({ command }));

    expect(state.status).toBe('submitting');
    expect(state.command).toEqual(command);
    expect(state.result).toBeNull();
  });

  it('marks success when the event is processed', () => {
    const state = transactionsReducer(undefined, TransactionsActions.transactionResolved({ result: processed }));

    expect(state.status).toBe('success');
    expect(state.result).toEqual(processed);
    expect(state.error).toBeNull();
  });

  it('maps a rejection to a business message', () => {
    const state = transactionsReducer(undefined, TransactionsActions.transactionResolved({ result: rejected }));

    expect(state.status).toBe('error');
    expect(state.error).toEqual({
      code: 'INSUFFICIENT_FUNDS',
      message: 'Saldo insuficiente para esta movimentação.',
    });
  });

  it('stores the failure message', () => {
    const state = transactionsReducer(
      undefined,
      TransactionsActions.submitTransactionFailure({ message: 'Sem conexão', code: 'NETWORK_ERROR' }),
    );

    expect(state.status).toBe('error');
    expect(state.error).toEqual({ message: 'Sem conexão', code: 'NETWORK_ERROR' });
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
