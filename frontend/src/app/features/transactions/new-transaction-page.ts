import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButton } from '@angular/material/button';
import { Store } from '@ngrx/store';
import { ProcessTransactionCommand, TransactionType } from '../../core/models/transaction';
import { CurrencyBrlPipe } from '../../shared/pipes/currency-brl.pipe';
import { Icon } from '../../shared/ui/icon/icon';
import * as AccountsActions from '../accounts/state/accounts.actions';
import { selectAccounts } from '../accounts/state/accounts.selectors';
import * as TransactionsActions from './state/transactions.actions';
import {
  selectSubmissionError,
  selectSubmissionResult,
  selectSubmissionStatus,
  selectSubmitting,
} from './state/transactions.selectors';

@Component({
  selector: 'app-new-transaction-page',
  imports: [ReactiveFormsModule, RouterLink, MatButton, CurrencyBrlPipe, Icon],
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

  readonly form = this.formBuilder.nonNullable.group({
    accountId: ['', Validators.required],
    type: ['CREDIT' as TransactionType, Validators.required],
    amount: [0, [Validators.required, Validators.min(0.01)]],
    occurredAt: [this.nowForInput(), Validators.required],
  });

  ngOnInit(): void {
    this.store.dispatch(AccountsActions.loadAccounts());
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
      eventId: crypto.randomUUID(),
      accountId,
      type,
      amount: Number(amount),
      occurredAt: new Date(occurredAt).toISOString(),
    };

    this.store.dispatch(TransactionsActions.submitTransaction({ command }));
  }

  submitAnother(): void {
    this.store.dispatch(TransactionsActions.clearTransactionResult());
    this.store.dispatch(AccountsActions.loadAccounts());
    this.form.patchValue({ amount: 0, occurredAt: this.nowForInput() });
  }

  private nowForInput(): string {
    const now = new Date();
    const local = new Date(now.getTime() - now.getTimezoneOffset() * 60_000);

    return local.toISOString().slice(0, 16);
  }
}
