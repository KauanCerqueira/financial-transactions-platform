import { createReducer, on } from '@ngrx/store';
import { Account } from '../../../core/models/account';
import { AccountsSummary } from '../../../core/models/accounts-summary';
import { LoadStatus } from '../../../core/models/load-status';
import * as AccountsActions from './accounts.actions';

export const accountsFeatureKey = 'accounts';

export interface AccountsState {
  accounts: Account[];
  summary: AccountsSummary | null;
  status: LoadStatus;
  error: string | null;
}

const initialState: AccountsState = {
  accounts: [],
  summary: null,
  status: 'idle',
  error: null,
};

export const accountsReducer = createReducer(
  initialState,
  on(AccountsActions.loadAccounts, (state) => ({ ...state, status: 'loading' as LoadStatus, error: null })),
  on(AccountsActions.loadAccountsSuccess, (state, { accounts, summary }) => ({
    ...state,
    accounts,
    summary,
    status: 'loaded' as LoadStatus,
    error: null,
  })),
  on(AccountsActions.loadAccountsFailure, (state, { error }) => ({
    ...state,
    status: 'error' as LoadStatus,
    error,
  })),
);
