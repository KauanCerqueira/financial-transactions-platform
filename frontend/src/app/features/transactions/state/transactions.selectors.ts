import { createFeatureSelector, createSelector } from '@ngrx/store';
import { TransactionFormState, transactionsFeatureKey } from './transactions.reducer';

export const selectTransactionFormState = createFeatureSelector<TransactionFormState>(transactionsFeatureKey);

export const selectSubmissionStatus = createSelector(selectTransactionFormState, (state) => state.status);

export const selectSubmissionResult = createSelector(selectTransactionFormState, (state) => state.result);

export const selectSubmissionError = createSelector(selectTransactionFormState, (state) => state.error);
export const selectSubmittedCommand = createSelector(selectTransactionFormState, (state) => state.command);

export const selectSubmitting = createSelector(selectSubmissionStatus, (status) => status === 'submitting');

export const selectProcessing = createSelector(selectSubmissionStatus, (status) => status === 'processing');
