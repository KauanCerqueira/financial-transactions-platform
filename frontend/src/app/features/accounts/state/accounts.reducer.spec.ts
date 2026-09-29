import { Account } from '../../../core/models/account';
import * as AccountsActions from './accounts.actions';
import { accountsReducer, AccountsState } from './accounts.reducer';

const accounts: Account[] = [
  { id: 'a1', holderName: 'Ana Souza', balance: 100, createdAt: '2026-01-30T10:00:00Z' },
];

describe('accountsReducer', () => {
  it('starts idle and empty', () => {
    const state = accountsReducer(undefined, { type: 'unknown' });

    expect(state).toEqual<AccountsState>({ accounts: [], status: 'idle', error: null });
  });

  it('sets loading when asked to load', () => {
    const state = accountsReducer(undefined, AccountsActions.loadAccounts());

    expect(state.status).toBe('loading');
    expect(state.error).toBeNull();
  });

  it('stores the accounts on success', () => {
    const state = accountsReducer(undefined, AccountsActions.loadAccountsSuccess({ accounts }));

    expect(state.status).toBe('loaded');
    expect(state.accounts).toEqual(accounts);
  });

  it('stores the error on failure', () => {
    const state = accountsReducer(undefined, AccountsActions.loadAccountsFailure({ error: 'Sem conexão' }));

    expect(state.status).toBe('error');
    expect(state.error).toBe('Sem conexão');
    expect(state.accounts).toEqual([]);
  });
});
