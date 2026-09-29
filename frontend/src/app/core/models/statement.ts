import { PagedResult } from './paged-result';
import { Transaction } from './transaction';

export interface StatementSummary {
  creditCount: number;
  creditTotal: number;
  debitCount: number;
  debitTotal: number;
}

export interface Statement {
  page: PagedResult<Transaction>;
  summary: StatementSummary;
}
