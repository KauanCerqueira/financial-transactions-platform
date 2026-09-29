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

    expect(state).toEqual({
      accounts: [],
      summary: null,
      selectedAccountId: null,
      status: 'idle',
      error: null,
      createStatus: 'idle',
      createError: null,
      createdAccount: null,
    } satisfies AccountsState);
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

  it('selects the first account by default', () => {
    const state = accountsReducer(undefined, AccountsActions.loadAccountsSuccess({ accounts, summary }));

    expect(state.selectedAccountId).toBe('a1');
  });

  it('keeps the selected account when it is still in the list', () => {
    const selected = accountsReducer(undefined, AccountsActions.selectAccount({ accountId: 'a1' }));
    const state = accountsReducer(selected, AccountsActions.loadAccountsSuccess({ accounts, summary }));

    expect(state.selectedAccountId).toBe('a1');
  });

  it('falls back to the first account when the selection is gone', () => {
    const selected = accountsReducer(undefined, AccountsActions.selectAccount({ accountId: 'gone' }));
    const state = accountsReducer(selected, AccountsActions.loadAccountsSuccess({ accounts, summary }));

    expect(state.selectedAccountId).toBe('a1');
  });

  it('drops the selection when there are no accounts', () => {
    const selected = accountsReducer(undefined, AccountsActions.selectAccount({ accountId: 'a1' }));
    const state = accountsReducer(
      selected,
      AccountsActions.loadAccountsSuccess({ accounts: [], summary: { accounts: 0, totalBalance: 0, transactions: 0 } }),
    );

    expect(state.selectedAccountId).toBeNull();
  });

  it('marks the creation as in progress and clears the previous error', () => {
    const failed = accountsReducer(
      undefined,
      AccountsActions.createAccountFailure({ message: 'Falhou', code: 'HTTP_400' }),
    );
    const state = accountsReducer(
      failed,
      AccountsActions.createAccount({ holderName: 'Joana', initialBalance: 10 }),
    );

    expect(state.createStatus).toBe('creating');
    expect(state.createError).toBeNull();
  });

  it('appends and selects the new account on success', () => {
    const loaded = accountsReducer(undefined, AccountsActions.loadAccountsSuccess({ accounts, summary }));
    const created: Account = {
      id: 'a2',
      holderName: 'Bruno Lima',
      balance: 250,
      createdAt: '2026-01-30T11:00:00Z',
    };

    const state = accountsReducer(loaded, AccountsActions.createAccountSuccess({ account: created }));

    expect(state.createStatus).toBe('created');
    expect(state.selectedAccountId).toBe('a2');
    expect(state.createdAccount).toEqual(created);
    expect(state.accounts.map((account) => account.holderName)).toEqual(['Ana Souza', 'Bruno Lima']);
  });

  it('keeps the accounts sorted by holder when a new one arrives', () => {
    const first = accountsReducer(
      undefined,
      AccountsActions.createAccountSuccess({
        account: { id: 'z1', holderName: 'Zeca Prado', balance: 0, createdAt: '2026-01-30T11:00:00Z' },
      }),
    );

    const state = accountsReducer(
      first,
      AccountsActions.createAccountSuccess({
        account: { id: 'a0', holderName: 'Abel Costa', balance: 0, createdAt: '2026-01-30T11:05:00Z' },
      }),
    );

    expect(state.accounts.map((account) => account.holderName)).toEqual(['Abel Costa', 'Zeca Prado']);
  });

  it('stores the creation error on failure', () => {
    const state = accountsReducer(
      undefined,
      AccountsActions.createAccountFailure({ message: 'O titular é obrigatório.', code: 'HTTP_400' }),
    );

    expect(state.createStatus).toBe('error');
    expect(state.createError).toEqual({ message: 'O titular é obrigatório.', code: 'HTTP_400' });
  });

  it('resets the creation state', () => {
    const created = accountsReducer(
      undefined,
      AccountsActions.createAccountSuccess({ account: accounts[0] }),
    );

    const state = accountsReducer(created, AccountsActions.resetCreateAccount());

    expect(state.createStatus).toBe('idle');
    expect(state.createError).toBeNull();
    expect(state.createdAccount).toBeNull();
  });
});
