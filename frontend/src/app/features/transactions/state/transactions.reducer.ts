import { createReducer, on } from '@ngrx/store';
import { ProcessedTransaction } from '../../../core/models/processed-transaction';
import * as TransactionsActions from './transactions.actions';

export const transactionsFeatureKey = 'transactionForm';

export type SubmissionStatus = 'idle' | 'submitting' | 'success' | 'error';

export interface TransactionFormState {
  status: SubmissionStatus;
  result: ProcessedTransaction | null;
  error: { message: string; code: string } | null;
}

const initialState: TransactionFormState = {
  status: 'idle',
  result: null,
  error: null,
};

export const transactionsReducer = createReducer(
  initialState,
  on(TransactionsActions.submitTransaction, (state) => ({
    ...state,
    status: 'submitting' as SubmissionStatus,
    result: null,
    error: null,
  })),
  on(TransactionsActions.submitTransactionSuccess, (state, { result }) => ({
    ...state,
    status: 'success' as SubmissionStatus,
    result,
    error: null,
  })),
  on(TransactionsActions.submitTransactionFailure, (state, { message, code }) => ({
    ...state,
    status: 'error' as SubmissionStatus,
    result: null,
    error: { message, code },
  })),
  on(TransactionsActions.clearTransactionResult, () => initialState),
);
