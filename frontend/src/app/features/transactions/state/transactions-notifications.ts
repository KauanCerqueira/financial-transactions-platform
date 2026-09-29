import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { tap } from 'rxjs';
import { messageForCode } from '../../../core/models/api-messages';
import { TransactionAccepted } from '../../../core/models/transaction-event';
import { Toast } from '../../../core/ui/toast';
import * as TransactionsActions from './transactions.actions';

@Injectable()
export class TransactionsNotifications {
  private readonly actions = inject(Actions);
  private readonly toast = inject(Toast);
  private readonly router = inject(Router);

  readonly resolved = createEffect(
    () =>
      this.actions.pipe(
        ofType(TransactionsActions.transactionResolved),
        tap(({ result }) => this.notify(result)),
      ),
    { dispatch: false },
  );

  readonly failed = createEffect(
    () =>
      this.actions.pipe(
        ofType(TransactionsActions.submitTransactionFailure),
        tap(({ message }) => this.toast.showError(message)),
      ),
    { dispatch: false },
  );

  private notify(result: TransactionAccepted): void {
    if (result.status === 'PROCESSED' && result.transaction) {
      const accountId = result.transaction.accountId;
      const openStatement = () => void this.router.navigate(['/accounts', accountId, 'statement']);

      if (result.alreadyProcessed) {
        this.toast.showInfo('Evento já processado — o saldo não mudou.', 'Ver extrato', openStatement);
        return;
      }

      this.toast.showSuccess('Transação enviada com sucesso!', 'Ver extrato', openStatement);

      return;
    }

    if (result.status === 'REJECTED') {
      this.toast.showError(messageForCode(result.rejectionCode, 'O lançamento foi rejeitado.'));
    }
  }
}
