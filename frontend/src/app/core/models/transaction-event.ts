import { Transaction } from './transaction';

export type TransactionEventStatus = 'PENDING' | 'PROCESSED' | 'REJECTED';

export interface TransactionAccepted {
  eventId: string;
  status: TransactionEventStatus;
  rejectionCode: string | null;
  transaction: Transaction | null;
  alreadyProcessed: boolean;
}
