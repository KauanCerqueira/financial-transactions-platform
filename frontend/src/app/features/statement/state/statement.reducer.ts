import { createReducer, on } from '@ngrx/store';
import { LoadStatus } from '../../../core/models/load-status';
import { emptyStatementFilters, StatementFilters } from '../../../core/models/statement-filters';
import { Transaction } from '../../../core/models/transaction';
import * as StatementActions from './statement.actions';

export const statementFeatureKey = 'statement';

export interface StatementState {
  items: Transaction[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  filters: StatementFilters;
  status: LoadStatus;
  error: string | null;
}

const initialState: StatementState = {
  items: [],
  page: 1,
  pageSize: 10,
  totalItems: 0,
  totalPages: 0,
  filters: emptyStatementFilters,
  status: 'idle',
  error: null,
};

export const statementReducer = createReducer(
  initialState,
  on(StatementActions.loadStatement, (state, { page, pageSize, filters }) => ({
    ...state,
    page,
    pageSize,
    filters: filters ?? emptyStatementFilters,
    status: 'loading' as LoadStatus,
    error: null,
  })),
  on(StatementActions.loadStatementSuccess, (state, { result }) => ({
    ...state,
    items: result.items,
    page: result.page,
    pageSize: result.pageSize,
    totalItems: result.totalItems,
    totalPages: result.totalPages,
    status: 'loaded' as LoadStatus,
    error: null,
  })),
  on(StatementActions.loadStatementFailure, (state, { error }) => ({
    ...state,
    status: 'error' as LoadStatus,
    error,
  })),
);
