import { createAction, props } from '@ngrx/store';
import { Account } from '../../../core/models/account';

export const loadAccounts = createAction('[Accounts] Load');
export const loadAccountsSuccess = createAction('[Accounts] Load success', props<{ accounts: Account[] }>());
export const loadAccountsFailure = createAction('[Accounts] Load failure', props<{ error: string }>());
