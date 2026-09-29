import { createAction, props } from '@ngrx/store';
import { Account } from '../../../core/models/account';
import { AccountsSummary } from '../../../core/models/accounts-summary';

export const loadAccounts = createAction('[Accounts] Load');
export const refreshAccounts = createAction('[Accounts] Refresh');
export const loadAccountsSuccess = createAction(
  '[Accounts] Load success',
  props<{ accounts: Account[]; summary: AccountsSummary }>(),
);
export const loadAccountsFailure = createAction('[Accounts] Load failure', props<{ error: string }>());

export const selectAccount = createAction('[Accounts] Select', props<{ accountId: string }>());

export const createAccount = createAction(
  '[Accounts] Create',
  props<{ holderName: string; initialBalance: number }>(),
);
export const createAccountSuccess = createAction(
  '[Accounts] Create success',
  props<{ account: Account }>(),
);
export const createAccountFailure = createAction(
  '[Accounts] Create failure',
  props<{ message: string; code: string }>(),
);
export const resetCreateAccount = createAction('[Accounts] Reset create');
