import { createAction, props } from '@ngrx/store';
import { ProcessedTransaction } from '../../../core/models/processed-transaction';
import { ProcessTransactionCommand } from '../../../core/models/transaction';

export const submitTransaction = createAction(
  '[Transaction form] Submit',
  props<{ command: ProcessTransactionCommand }>(),
);
export const submitTransactionSuccess = createAction(
  '[Transaction form] Submit success',
  props<{ result: ProcessedTransaction }>(),
);
export const submitTransactionFailure = createAction(
  '[Transaction form] Submit failure',
  props<{ message: string; code: string }>(),
);
export const clearTransactionResult = createAction('[Transaction form] Clear result');
