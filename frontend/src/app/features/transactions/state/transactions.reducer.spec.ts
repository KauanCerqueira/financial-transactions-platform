import { TransactionAccepted } from '../../../core/models/transaction-event';
import * as TransactionsActions from './transactions.actions';
import { transactionsReducer } from './transactions.reducer';

const command = { eventId: 'e1', accountId: 'a1', type: 'CREDIT' as const, amount: 100, occurredAt: '2026-01-30T10:00:00Z' };

const pending: TransactionAccepted = {
  eventId: 'e1',
  status: 'PENDING',
  rejectionCode: null,
  transaction: null,
  alreadyProcessed: false,
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
  alreadyProcessed: true,
};

describe('transactionsReducer', () => {
  it('marks submitting on submit', () => {
    const state = transactionsReducer(undefined, TransactionsActions.submitTransaction({ command }));

    expect(state.status).toBe('submitting');
    expect(state.command).toEqual(command);
    expect(state.result).toBeNull();
  });

  it('marks processing while the queue still has the event pending', () => {
    const state = transactionsReducer(undefined, TransactionsActions.transactionAccepted({ result: pending }));

    expect(state.status).toBe('processing');
    expect(state.result).toEqual(pending);
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

  it('clears back to idle', () => {
    const failed = transactionsReducer(undefined, TransactionsActions.transactionResolved({ result: rejected }));

    const state = transactionsReducer(failed, TransactionsActions.clearTransactionResult());

    expect(state.status).toBe('idle');
    expect(state.error).toBeNull();
  });

  it('shows a recoverable error when the same event takes too long', () => {
    const submitted = transactionsReducer(undefined, TransactionsActions.submitTransaction({ command }));
    const processing = transactionsReducer(submitted, TransactionsActions.transactionAccepted({ result: pending }));

    const timedOut = transactionsReducer(
      processing,
      TransactionsActions.transactionPollingTimedOut({ eventId: command.eventId }),
    );

    expect(timedOut.status).toBe('error');
    expect(timedOut.command).toEqual(command);
    expect(timedOut.error?.code).toBe('PROCESSING_TIMEOUT');
  });
});
