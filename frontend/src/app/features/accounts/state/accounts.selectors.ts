import { createFeatureSelector, createSelector } from '@ngrx/store';
import { AccountsState, accountsFeatureKey } from './accounts.reducer';

export const selectAccountsState = createFeatureSelector<AccountsState>(accountsFeatureKey);

export const selectAccounts = createSelector(selectAccountsState, (state) => state.accounts);

export const selectAccountsStatus = createSelector(selectAccountsState, (state) => state.status);

export const selectAccountsError = createSelector(selectAccountsState, (state) => state.error);

export const selectAccountsLoading = createSelector(selectAccountsStatus, (status) => status === 'loading');

export const selectTotalBalance = createSelector(selectAccounts, (accounts) =>
  accounts.reduce((total, account) => total + account.balance, 0),
);
