import { createAction, props } from '@ngrx/store';
import { ProcessTransactionCommand } from '../../../core/models/transaction';
import { TransactionAccepted } from '../../../core/models/transaction-event';

export const submitTransaction = createAction(
  '[Transaction form] Submit',
  props<{ command: ProcessTransactionCommand }>(),
);
export const transactionAccepted = createAction(
  '[Transaction form] Accepted',
  props<{ result: TransactionAccepted }>(),
);
export const transactionResolved = createAction(
  '[Transaction form] Resolved',
  props<{ result: TransactionAccepted }>(),
);
export const submitTransactionFailure = createAction(
  '[Transaction form] Failure',
  props<{ message: string; code: string }>(),
);
export const clearTransactionResult = createAction('[Transaction form] Clear');
