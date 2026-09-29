import { createFeatureSelector, createSelector } from '@ngrx/store';
import { StatementState, statementFeatureKey } from './statement.reducer';

export const selectStatementState = createFeatureSelector<StatementState>(statementFeatureKey);

export const selectStatementItems = createSelector(selectStatementState, (state) => state.items);

export const selectStatementPage = createSelector(selectStatementState, (state) => state.page);

export const selectStatementPageSize = createSelector(selectStatementState, (state) => state.pageSize);

export const selectStatementTotalItems = createSelector(selectStatementState, (state) => state.totalItems);

export const selectStatementFilters = createSelector(selectStatementState, (state) => state.filters);

export const selectStatementStatus = createSelector(selectStatementState, (state) => state.status);

export const selectStatementError = createSelector(selectStatementState, (state) => state.error);

export const selectStatementLoading = createSelector(selectStatementStatus, (status) => status === 'loading');
