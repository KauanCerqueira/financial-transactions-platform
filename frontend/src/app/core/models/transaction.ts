export type TransactionType = 'CREDIT' | 'DEBIT';

export interface Transaction {
  id: string;
  eventId: string;
  accountId: string;
  type: TransactionType;
  amount: number;
  occurredAt: string;
  balanceAfter: number;
  recordedAt: string;
}

export interface ProcessTransactionCommand {
  eventId: string;
  accountId: string;
  type: TransactionType;
  amount: number;
  occurredAt: string;
}
