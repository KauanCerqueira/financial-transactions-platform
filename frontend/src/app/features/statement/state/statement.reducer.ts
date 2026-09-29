import { createReducer, on } from '@ngrx/store';
import { LoadStatus } from '../../../core/models/load-status';
import { StatementSummary } from '../../../core/models/statement';
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
  summary: StatementSummary | null;
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
  summary: null,
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
    summary: null,
    status: 'loading' as LoadStatus,
    error: null,
  })),
  on(StatementActions.loadStatementSuccess, (state, { result }) => ({
    ...state,
    items: result.page.items,
    page: result.page.page,
    pageSize: result.page.pageSize,
    totalItems: result.page.totalItems,
    totalPages: result.page.totalPages,
    summary: result.summary,
    status: 'loaded' as LoadStatus,
    error: null,
  })),
  on(StatementActions.loadStatementFailure, (state, { error }) => ({
    ...state,
    status: 'error' as LoadStatus,
    error,
  })),
);
