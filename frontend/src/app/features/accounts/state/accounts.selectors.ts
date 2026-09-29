import { createFeatureSelector, createSelector } from '@ngrx/store';
import { AccountsState, accountsFeatureKey } from './accounts.reducer';

export const selectAccountsState = createFeatureSelector<AccountsState>(accountsFeatureKey);

export const selectAccounts = createSelector(selectAccountsState, (state) => state.accounts);

export const selectAccountsSummary = createSelector(selectAccountsState, (state) => state.summary);

export const selectAccountsStatus = createSelector(selectAccountsState, (state) => state.status);

export const selectAccountsError = createSelector(selectAccountsState, (state) => state.error);

export const selectAccountsLoading = createSelector(selectAccountsStatus, (status) => status === 'loading');

export const selectSelectedAccountId = createSelector(
  selectAccountsState,
  (state) => state.selectedAccountId ?? '',
);

export const selectCreateAccountStatus = createSelector(
  selectAccountsState,
  (state) => state.createStatus,
);

export const selectCreateAccountError = createSelector(
  selectAccountsState,
  (state) => state.createError,
);

export const selectCreatedAccount = createSelector(
  selectAccountsState,
  (state) => state.createdAccount,
);
