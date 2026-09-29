import { Component, OnInit, computed, effect, inject, input, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButton } from '@angular/material/button';
import { Store } from '@ngrx/store';
import { ProcessTransactionCommand, TransactionType } from '../../core/models/transaction';
import { CurrencyBrlPipe } from '../../shared/pipes/currency-brl.pipe';
import { ShortIdPipe } from '../../shared/pipes/short-id.pipe';
import { Icon } from '../../shared/ui/icon/icon';
import { forgetPendingTransaction, recoverPendingTransaction } from './pending-transaction';
import * as AccountsActions from '../accounts/state/accounts.actions';
import { selectAccounts } from '../accounts/state/accounts.selectors';
import * as TransactionsActions from './state/transactions.actions';
import {
  selectSubmissionError,
  selectSubmissionResult,
  selectSubmissionStatus,
  selectSubmittedCommand,
  selectSubmitting,
} from './state/transactions.selectors';

@Component({
  selector: 'app-new-transaction-page',
  imports: [ReactiveFormsModule, RouterLink, MatButton, CurrencyBrlPipe, ShortIdPipe, Icon],
  templateUrl: './new-transaction-page.html',
})
export class NewTransactionPage implements OnInit {
  private readonly store = inject(Store);
  private readonly formBuilder = inject(FormBuilder);

  readonly accounts = this.store.selectSignal(selectAccounts);
  readonly status = this.store.selectSignal(selectSubmissionStatus);
  readonly submitting = this.store.selectSignal(selectSubmitting);
  readonly result = this.store.selectSignal(selectSubmissionResult);
  readonly error = this.store.selectSignal(selectSubmissionError);
  readonly submittedCommand = this.store.selectSignal(selectSubmittedCommand);
  readonly canRetry = computed(() => {
    const code = this.error()?.code ?? '';

    return code === 'NETWORK_ERROR' || code.startsWith('HTTP_5');
  });
  readonly initialAccountId = input('');
  readonly eventId = signal(crypto.randomUUID());

  readonly form = this.formBuilder.nonNullable.group({
    accountId: ['', Validators.required],
    type: ['CREDIT' as TransactionType, Validators.required],
    amount: [0, [Validators.required, Validators.min(0.01)]],
    occurredAt: [this.nowForInput(), Validators.required],
  });

  constructor() {
    effect(() => {
      const accountId = this.initialAccountId();
      if (accountId) this.form.controls.accountId.setValue(accountId);
    });
    effect(() => {
      if (this.status() === 'success') this.store.dispatch(AccountsActions.loadAccounts());
    });
  }

  ngOnInit(): void {
    this.store.dispatch(AccountsActions.loadAccounts());

    const pending = recoverPendingTransaction();

    if (pending) {
      this.store.dispatch(TransactionsActions.submitTransaction({ command: pending }));
    }
  }

  selectType(type: TransactionType): void {
    this.form.controls.type.setValue(type);
  }

  accountName(accountId: string): string {
    return this.accounts().find((account) => account.id === accountId)?.holderName ?? accountId;
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { accountId, type, amount, occurredAt } = this.form.getRawValue();
    const command: ProcessTransactionCommand = {
      eventId: this.eventId(),
      accountId,
      type,
      amount: Number(amount),
      occurredAt: new Date(occurredAt).toISOString(),
    };

    this.store.dispatch(TransactionsActions.submitTransaction({ command }));
  }

  submitAnother(): void {
    this.eventId.set(crypto.randomUUID());
    forgetPendingTransaction();
    this.store.dispatch(TransactionsActions.clearTransactionResult());
    this.store.dispatch(AccountsActions.loadAccounts());
    this.form.patchValue({ amount: 0, occurredAt: this.nowForInput() });
    this.form.markAsUntouched();
    this.form.markAsPristine();
  }

  retry(): void {
    const command = this.submittedCommand();

    if (command) {
      this.store.dispatch(TransactionsActions.submitTransaction({ command }));
    }
  }

  private nowForInput(): string {
    const now = new Date();
    const local = new Date(now.getTime() - now.getTimezoneOffset() * 60_000);

    return local.toISOString().slice(0, 16);
  }
}
