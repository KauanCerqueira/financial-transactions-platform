import { createAction, props } from '@ngrx/store';
import { Account } from '../../../core/models/account';
import { AccountsSummary } from '../../../core/models/accounts-summary';

export const loadAccounts = createAction('[Accounts] Load');
export const loadAccountsSuccess = createAction(
  '[Accounts] Load success',
  props<{ accounts: Account[]; summary: AccountsSummary }>(),
);
export const loadAccountsFailure = createAction('[Accounts] Load failure', props<{ error: string }>());
