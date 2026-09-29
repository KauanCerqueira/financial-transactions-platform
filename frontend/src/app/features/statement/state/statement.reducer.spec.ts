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
    const state = statementReducer(undefined, StatementActions.loadStatement({ accountId: 'a1', page: 3, pageSize: 20 }));

    expect(state.status).toBe('loading');
    expect(state.page).toBe(3);
    expect(state.pageSize).toBe(20);
  });

  it('stores the paged result on success', () => {
    const state = statementReducer(
      undefined,
      StatementActions.loadStatementSuccess({
        result: { items: [transaction], page: 2, pageSize: 10, totalItems: 25, totalPages: 3 },
      }),
    );

    expect(state.status).toBe('loaded');
    expect(state.items).toEqual([transaction]);
    expect(state.page).toBe(2);
    expect(state.totalItems).toBe(25);
    expect(state.totalPages).toBe(3);
  });

  it('stores the error on failure', () => {
    const state = statementReducer(undefined, StatementActions.loadStatementFailure({ error: 'Sem conexão' }));

    expect(state.status).toBe('error');
    expect(state.error).toBe('Sem conexão');
  });
});
