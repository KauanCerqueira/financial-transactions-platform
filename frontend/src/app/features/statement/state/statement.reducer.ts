import { createReducer, on } from '@ngrx/store';
import { LoadStatus } from '../../../core/models/load-status';
import { Transaction } from '../../../core/models/transaction';
import * as StatementActions from './statement.actions';

export const statementFeatureKey = 'statement';

export interface StatementState {
  items: Transaction[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  status: LoadStatus;
  error: string | null;
}

const initialState: StatementState = {
  items: [],
  page: 1,
  pageSize: 10,
  totalItems: 0,
  totalPages: 0,
  status: 'idle',
  error: null,
};

export const statementReducer = createReducer(
  initialState,
  on(StatementActions.loadStatement, (state, { page, pageSize }) => ({
    ...state,
    page,
    pageSize,
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
