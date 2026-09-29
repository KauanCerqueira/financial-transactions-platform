import { createAction, props } from '@ngrx/store';
import { Statement } from '../../../core/models/statement';
import { StatementFilters } from '../../../core/models/statement-filters';

export const loadStatement = createAction(
  '[Statement] Load',
  props<{ accountId: string; page: number; pageSize: number; filters: StatementFilters }>(),
);
export const loadStatementSuccess = createAction(
  '[Statement] Load success',
  props<{ result: Statement }>(),
);
export const loadStatementFailure = createAction('[Statement] Load failure', props<{ error: string }>());
