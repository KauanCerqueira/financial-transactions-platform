import { TransactionType } from './transaction';

export interface StatementFilters {
  type?: TransactionType | null;
  from?: string | null;
  to?: string | null;
}

export const emptyStatementFilters: StatementFilters = { type: null, from: null, to: null };
