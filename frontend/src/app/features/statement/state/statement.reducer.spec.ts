import { Transaction } from '../../../core/models/transaction';
import * as StatementActions from './statement.actions';
import { statementReducer } from './statement.reducer';

const transaction: Transaction = {
  id: 't1',
  eventId: 'e1',
  accountId: 'a1',
  type: 'CREDIT',
  amount: 100,
  occurredAt: '2026-01-30T10:00:00Z',
  balanceAfter: 100,
  recordedAt: '2026-01-30T10:00:00Z',
};

describe('statementReducer', () => {
  it('keeps the requested page and marks loading', () => {
    const state = statementReducer(
      undefined,
      StatementActions.loadStatement({ accountId: 'a1', page: 3, pageSize: 20, filters: { type: 'DEBIT' } }),
    );

    expect(state.status).toBe('loading');
    expect(state.page).toBe(3);
    expect(state.pageSize).toBe(20);
    expect(state.filters).toEqual({ type: 'DEBIT' });
  });

  it('stores the paged result on success', () => {
    const state = statementReducer(
      undefined,
      StatementActions.loadStatementSuccess({
        result: {
          page: { items: [transaction], page: 2, pageSize: 10, totalItems: 25, totalPages: 3 },
          summary: { creditCount: 1, creditTotal: 100, debitCount: 0, debitTotal: 0 },
        },
      }),
    );

    expect(state.status).toBe('loaded');
    expect(state.items).toEqual([transaction]);
    expect(state.page).toBe(2);
    expect(state.totalItems).toBe(25);
    expect(state.totalPages).toBe(3);
    expect(state.summary?.creditTotal).toBe(100);
  });

  it('stores the error on failure', () => {
    const state = statementReducer(undefined, StatementActions.loadStatementFailure({ error: 'Sem conexão' }));

    expect(state.status).toBe('error');
    expect(state.error).toBe('Sem conexão');
  });
});
