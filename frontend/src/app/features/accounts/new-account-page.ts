import { Component, computed, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButton } from '@angular/material/button';
import { RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { CurrencyBrlPipe } from '../../shared/pipes/currency-brl.pipe';
import { ShortIdPipe } from '../../shared/pipes/short-id.pipe';
import { Icon } from '../../shared/ui/icon/icon';
import * as AccountsActions from './state/accounts.actions';
import {
  selectCreateAccountError,
  selectCreateAccountStatus,
  selectCreatedAccount,
} from './state/accounts.selectors';

const MaximumHolderNameLength = 200;

@Component({
  selector: 'app-new-account-page',
  imports: [ReactiveFormsModule, RouterLink, MatButton, CurrencyBrlPipe, ShortIdPipe, Icon],
  templateUrl: './new-account-page.html',
})
export class NewAccountPage {
  private readonly store = inject(Store);

  readonly status = this.store.selectSignal(selectCreateAccountStatus);
  readonly error = this.store.selectSignal(selectCreateAccountError);
  readonly createdAccount = this.store.selectSignal(selectCreatedAccount);
  readonly submitting = computed(() => this.status() === 'creating');

  readonly form = inject(FormBuilder).nonNullable.group({
    holderName: [
      '',
      [Validators.required, Validators.maxLength(MaximumHolderNameLength), Validators.pattern(/\S/)],
    ],
    initialBalance: [0, [Validators.min(0)]],
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const values = this.form.getRawValue();

    this.store.dispatch(
      AccountsActions.createAccount({
        holderName: values.holderName.trim(),
        initialBalance: Number(values.initialBalance) || 0,
      }),
    );
  }

  createAnother(): void {
    this.store.dispatch(AccountsActions.resetCreateAccount());
    this.form.reset({ holderName: '', initialBalance: 0 });
  }
}
