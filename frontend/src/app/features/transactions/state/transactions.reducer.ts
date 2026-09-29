import { createReducer, on } from '@ngrx/store';
import { messageForCode } from '../../../core/models/api-messages';
import { TransactionAccepted } from '../../../core/models/transaction-event';
import * as TransactionsActions from './transactions.actions';

export const transactionsFeatureKey = 'transactionForm';

export type SubmissionStatus = 'idle' | 'submitting' | 'processing' | 'success' | 'error';

export interface TransactionFormState {
  status: SubmissionStatus;
  result: TransactionAccepted | null;
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
  on(TransactionsActions.transactionAccepted, (state, { result }) => ({
    ...state,
    result,
    status: result.status === 'PENDING' ? ('processing' as SubmissionStatus) : statusOf(result),
    error: result.status === 'REJECTED' ? rejectionOf(result) : null,
  })),
  on(TransactionsActions.transactionResolved, (state, { result }) => ({
    ...state,
    result,
    status: statusOf(result),
    error: result.status === 'REJECTED' ? rejectionOf(result) : null,
  })),
  on(TransactionsActions.submitTransactionFailure, (state, { message, code }) => ({
    ...state,
    status: 'error' as SubmissionStatus,
    result: null,
    error: { message, code },
  })),
  on(TransactionsActions.clearTransactionResult, () => initialState),
);

function statusOf(result: TransactionAccepted): SubmissionStatus {
  if (result.status === 'PROCESSED') {
    return 'success';
  }

  if (result.status === 'REJECTED') {
    return 'error';
  }

  return 'processing';
}

function rejectionOf(result: TransactionAccepted): { message: string; code: string } {
  const code = result.rejectionCode ?? 'REJECTED';

  return { code, message: messageForCode(code, 'O lançamento foi rejeitado.') };
}
