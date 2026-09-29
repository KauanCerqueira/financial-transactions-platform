import { Transaction } from './transaction';

export interface ProcessedTransaction {
  transaction: Transaction;
  alreadyProcessed: boolean;
}
