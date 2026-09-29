import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { MockStore, provideMockStore } from '@ngrx/store/testing';
import { Account } from '../../core/models/account';
import { NewTransactionPage } from './new-transaction-page';
import { SubmissionStatus } from './state/transactions.reducer';

const accounts: Account[] = [
  { id: 'a1', holderName: 'Ana Souza', balance: 100, createdAt: '2026-01-30T10:00:00Z' },
];

describe('NewTransactionPage', () => {
  beforeEach(() => sessionStorage.clear());

  function setup(submission?: { status: SubmissionStatus; message: string; code: string }) {
    TestBed.configureTestingModule({
      imports: [NewTransactionPage],
      providers: [
        provideRouter([]),
        provideMockStore({
          initialState: {
            accounts: { accounts, status: 'loaded', error: null },
            transactionForm: {
              command: submission ? {
                eventId: 'previous-event',
                accountId: 'a1',
                type: 'CREDIT',
                amount: 25,
                occurredAt: '2026-01-30T10:00:00Z',
              } : null,
              status: submission?.status ?? 'idle',
              result: null,
              error: submission ? { message: submission.message, code: submission.code } : null,
            },
          },
        }),
      ],
    });

    const store = TestBed.inject(MockStore);
    const dispatch = vi.spyOn(store, 'dispatch');
    const fixture = TestBed.createComponent(NewTransactionPage);
    fixture.detectChanges();

    return { fixture, component: fixture.componentInstance, store, dispatch };
  }

  it('does not dispatch when the form is invalid', () => {
    const { component, store } = setup();
    const dispatch = vi.spyOn(store, 'dispatch');
    dispatch.mockClear();

    component.submit();

    expect(dispatch).not.toHaveBeenCalled();
    expect(component.form.controls.accountId.touched).toBe(true);
  });

  it('dispatches a submit action with a fresh eventId and ISO date', () => {
    const { component, store } = setup();
    const dispatch = vi.spyOn(store, 'dispatch');
    dispatch.mockClear();

    component.form.patchValue({ accountId: 'a1', amount: 25.5, type: 'DEBIT' });
    component.submit();

    const calls = dispatch.mock.calls as unknown as unknown[][];
    const action = calls[0][0] as {
      type: string;
      command: { accountId: string; type: string; amount: number; eventId: string; occurredAt: string };
    };

    expect(action.type).toBe('[Transaction form] Submit');
    expect(action.command.accountId).toBe('a1');
    expect(action.command.type).toBe('DEBIT');
    expect(action.command.amount).toBe(25.5);
    expect(action.command.eventId).toMatch(/^[0-9a-f-]{36}$/);
    expect(action.command.occurredAt).toMatch(/Z$/);
  });

  it('renders the business error coming from the store', () => {
    const { fixture } = setup({
      status: 'error',
      message: 'Saldo insuficiente para esta movimentação.',
      code: 'INSUFFICIENT_FUNDS',
    });
    const element = fixture.nativeElement as HTMLElement;

    expect(element.querySelector('.state--error')).toBeTruthy();
    expect(element.textContent).toContain('INSUFFICIENT_FUNDS');
    expect(element.textContent).toContain('Saldo insuficiente para esta movimentação.');
  });

  it('retries a failed request using its original eventId', () => {
    const { component, store } = setup({ status: 'error', message: 'Sem conexão', code: 'NETWORK_ERROR' });
    const dispatch = vi.spyOn(store, 'dispatch');

    component.retry();

    expect(dispatch).toHaveBeenCalledWith(
      expect.objectContaining({ command: expect.objectContaining({ eventId: 'previous-event' }) }),
    );
  });

  it('recovers a pending command after reloading the page', () => {
    sessionStorage.setItem('financial-transactions:pending-command', JSON.stringify({
      eventId: 'saved-event', accountId: 'a1', type: 'CREDIT', amount: 25, occurredAt: '2026-01-30T10:00:00Z',
    }));
    const { dispatch } = setup();

    expect(dispatch).toHaveBeenCalledWith(
      expect.objectContaining({ command: expect.objectContaining({ eventId: 'saved-event' }) }),
    );
  });
});
