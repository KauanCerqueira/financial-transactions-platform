import { createReducer, on } from '@ngrx/store';
import { Account } from '../../../core/models/account';
import { AccountsSummary } from '../../../core/models/accounts-summary';
import { LoadStatus } from '../../../core/models/load-status';
import * as AccountsActions from './accounts.actions';

export const accountsFeatureKey = 'accounts';

export type CreateStatus = 'idle' | 'creating' | 'created' | 'error';

export interface AccountsState {
  accounts: Account[];
  summary: AccountsSummary | null;
  selectedAccountId: string | null;
  status: LoadStatus;
  error: string | null;
  createStatus: CreateStatus;
  createError: { message: string; code: string } | null;
  createdAccount: Account | null;
}

const initialState: AccountsState = {
  accounts: [],
  summary: null,
  selectedAccountId: null,
  status: 'idle',
  error: null,
  createStatus: 'idle',
  createError: null,
  createdAccount: null,
};

export const accountsReducer = createReducer(
  initialState,
  on(AccountsActions.loadAccounts, (state) => ({ ...state, status: 'loading' as LoadStatus, error: null })),
  on(AccountsActions.loadAccountsSuccess, (state, { accounts, summary }) => ({
    ...state,
    accounts,
    summary,
    selectedAccountId: keepSelection(state.selectedAccountId, accounts),
    status: 'loaded' as LoadStatus,
    error: null,
  })),
  on(AccountsActions.loadAccountsFailure, (state, { error }) => ({
    ...state,
    status: 'error' as LoadStatus,
    error,
  })),
  on(AccountsActions.selectAccount, (state, { accountId }) => ({
    ...state,
    selectedAccountId: accountId,
  })),
  on(AccountsActions.createAccount, (state) => ({
    ...state,
    createStatus: 'creating' as CreateStatus,
    createError: null,
    createdAccount: null,
  })),
  on(AccountsActions.createAccountSuccess, (state, { account }) => ({
    ...state,
    accounts: [
      ...state.accounts.filter((existing) => existing.id !== account.id),
      account,
    ].sort((left, right) => left.holderName.localeCompare(right.holderName, 'pt-BR')),
    selectedAccountId: account.id,
    createStatus: 'created' as CreateStatus,
    createError: null,
    createdAccount: account,
  })),
  on(AccountsActions.createAccountFailure, (state, { message, code }) => ({
    ...state,
    createStatus: 'error' as CreateStatus,
    createError: { message, code },
    createdAccount: null,
  })),
  on(AccountsActions.resetCreateAccount, (state) => ({
    ...state,
    createStatus: 'idle' as CreateStatus,
    createError: null,
    createdAccount: null,
  })),
);

function keepSelection(selectedAccountId: string | null, accounts: Account[]): string | null {
  if (selectedAccountId && accounts.some((account) => account.id === selectedAccountId)) {
    return selectedAccountId;
  }

  return accounts[0]?.id ?? null;
}
