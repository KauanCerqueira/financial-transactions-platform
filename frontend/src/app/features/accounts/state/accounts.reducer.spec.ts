import { Account } from '../../../core/models/account';
import { AccountsSummary } from '../../../core/models/accounts-summary';
import * as AccountsActions from './accounts.actions';
import { accountsReducer, AccountsState } from './accounts.reducer';

const accounts: Account[] = [
  { id: 'a1', holderName: 'Ana Souza', balance: 100, createdAt: '2026-01-30T10:00:00Z' },
];

const summary: AccountsSummary = { accounts: 1, totalBalance: 100, transactions: 5 };

describe('accountsReducer', () => {
  it('starts idle and empty', () => {
    const state = accountsReducer(undefined, { type: 'unknown' });

    expect(state).toEqual({ accounts: [], summary: null, status: 'idle', error: null } satisfies AccountsState);
  });

  it('sets loading when asked to load', () => {
    const state = accountsReducer(undefined, AccountsActions.loadAccounts());

    expect(state.status).toBe('loading');
    expect(state.error).toBeNull();
  });

  it('stores the accounts and the summary on success', () => {
    const state = accountsReducer(undefined, AccountsActions.loadAccountsSuccess({ accounts, summary }));

    expect(state.status).toBe('loaded');
    expect(state.accounts).toEqual(accounts);
    expect(state.summary).toEqual(summary);
  });

  it('stores the error on failure', () => {
    const state = accountsReducer(undefined, AccountsActions.loadAccountsFailure({ error: 'Sem conexão' }));

    expect(state.status).toBe('error');
    expect(state.error).toBe('Sem conexão');
    expect(state.accounts).toEqual([]);
  });
});
