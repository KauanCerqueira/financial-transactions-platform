import { createAction, props } from '@ngrx/store';
import { PagedResult } from '../../../core/models/paged-result';
import { StatementFilters } from '../../../core/models/statement-filters';
import { Transaction } from '../../../core/models/transaction';

export const loadStatement = createAction(
  '[Statement] Load',
  props<{ accountId: string; page: number; pageSize: number; filters: StatementFilters }>(),
);
export const loadStatementSuccess = createAction(
  '[Statement] Load success',
  props<{ result: PagedResult<Transaction> }>(),
);
export const loadStatementFailure = createAction('[Statement] Load failure', props<{ error: string }>());
