import { createReducer, on } from '@ngrx/store';
import { messageForCode } from '../../../core/models/api-messages';
import { ProcessTransactionCommand } from '../../../core/models/transaction';
import { TransactionAccepted } from '../../../core/models/transaction-event';
import * as TransactionsActions from './transactions.actions';

export const transactionsFeatureKey = 'transactionForm';

export type SubmissionStatus = 'idle' | 'submitting' | 'success' | 'error';

export interface TransactionFormState {
  command: ProcessTransactionCommand | null;
  status: SubmissionStatus;
  result: TransactionAccepted | null;
  error: { message: string; code: string } | null;
}

const initialState: TransactionFormState = {
  command: null,
  status: 'idle',
  result: null,
  error: null,
};

export const transactionsReducer = createReducer(
  initialState,
  on(TransactionsActions.submitTransaction, (state, { command }) => ({
    ...state,
    command,
    status: 'submitting' as SubmissionStatus,
    result: null,
    error: null,
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
  return result.status === 'PROCESSED' ? 'success' : 'error';
}

function rejectionOf(result: TransactionAccepted): { message: string; code: string } {
  const code = result.rejectionCode ?? 'REJECTED';

  return { code, message: messageForCode(code, 'O lançamento foi rejeitado.') };
}
