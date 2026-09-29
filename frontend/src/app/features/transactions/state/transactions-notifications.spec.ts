import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideMockActions } from '@ngrx/effects/testing';
import { ReplaySubject } from 'rxjs';
import { TransactionAccepted } from '../../../core/models/transaction-event';
import { Toast } from '../../../core/ui/toast';
import * as TransactionsActions from './transactions.actions';
import { TransactionsNotifications } from './transactions-notifications';

const processed: TransactionAccepted = {
  eventId: 'e1',
  status: 'PROCESSED',
  rejectionCode: null,
  transaction: {
    id: 't1',
    eventId: 'e1',
    accountId: 'a1',
    type: 'CREDIT',
    amount: 100,
    occurredAt: '2026-01-30T10:00:00Z',
    balanceAfter: 200,
    recordedAt: '2026-01-30T10:00:00Z',
  },
  alreadyProcessed: false,
};

describe('TransactionsNotifications', () => {
  let actions$: ReplaySubject<unknown>;
  let notifications: TransactionsNotifications;
  let toast: { showSuccess: jasmine.Spy; showInfo: jasmine.Spy; showError: jasmine.Spy };

  beforeEach(() => {
    actions$ = new ReplaySubject(1);
    toast = {
      showSuccess: jasmine.createSpy('showSuccess'),
      showInfo: jasmine.createSpy('showInfo'),
      showError: jasmine.createSpy('showError'),
    };

    TestBed.configureTestingModule({
      providers: [
        TransactionsNotifications,
        provideRouter([]),
        provideMockActions(() => actions$),
        { provide: Toast, useValue: toast },
      ],
    });

    notifications = TestBed.inject(TransactionsNotifications);
  });

  function dispatch(action: unknown): void {
    const subscription = notifications.resolved.subscribe();
    actions$.next(action);
    subscription.unsubscribe();
  }

  it('shows the success toast for a new transaction', () => {
    dispatch(TransactionsActions.transactionResolved({ result: processed }));

    expect(toast.showSuccess).toHaveBeenCalledWith('Transação enviada com sucesso!', 'Ver extrato', jasmine.any(Function));
    expect(toast.showInfo).not.toHaveBeenCalled();
  });

  it('shows an info toast when the event was already processed', () => {
    dispatch(TransactionsActions.transactionResolved({ result: { ...processed, alreadyProcessed: true } }));

    expect(toast.showInfo).toHaveBeenCalledWith(
      'Evento já processado — o saldo não mudou.',
      'Ver extrato',
      jasmine.any(Function),
    );
    expect(toast.showSuccess).not.toHaveBeenCalled();
  });
});
