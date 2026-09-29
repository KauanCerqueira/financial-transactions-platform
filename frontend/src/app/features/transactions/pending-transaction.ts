import { ProcessTransactionCommand } from '../../core/models/transaction';

const storageKey = 'financial-transactions:pending-command';

export function rememberPendingTransaction(command: ProcessTransactionCommand): void {
  sessionStorage.setItem(storageKey, JSON.stringify(command));
}

export function recoverPendingTransaction(): ProcessTransactionCommand | null {
  const saved = sessionStorage.getItem(storageKey);

  if (!saved) {
    return null;
  }

  try {
    const command: unknown = JSON.parse(saved);

    if (typeof command === 'object' && command !== null
      && 'eventId' in command && typeof command.eventId === 'string'
      && 'accountId' in command && typeof command.accountId === 'string'
      && 'type' in command && (command.type === 'CREDIT' || command.type === 'DEBIT')
      && 'amount' in command && typeof command.amount === 'number'
      && 'occurredAt' in command && typeof command.occurredAt === 'string') {
      return command as ProcessTransactionCommand;
    }

    forgetPendingTransaction();
    return null;
  } catch (error) {
    if (error instanceof SyntaxError) {
      forgetPendingTransaction();
      return null;
    }

    throw error;
  }
}

export function forgetPendingTransaction(): void {
  sessionStorage.removeItem(storageKey);
}
