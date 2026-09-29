import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButton } from '@angular/material/button';
import { Store } from '@ngrx/store';
import { CurrencyBrlPipe } from '../../shared/pipes/currency-brl.pipe';
import * as AccountsActions from './state/accounts.actions';
import {
  selectAccounts,
  selectAccountsError,
  selectAccountsLoading,
  selectAccountsStatus,
  selectTotalBalance,
} from './state/accounts.selectors';

@Component({
  selector: 'app-accounts-page',
  imports: [RouterLink, MatButton, CurrencyBrlPipe],
  templateUrl: './accounts-page.html',
})
export class AccountsPage implements OnInit {
  private readonly store = inject(Store);

  readonly accounts = this.store.selectSignal(selectAccounts);
  readonly status = this.store.selectSignal(selectAccountsStatus);
  readonly error = this.store.selectSignal(selectAccountsError);
  readonly loading = this.store.selectSignal(selectAccountsLoading);
  readonly totalBalance = this.store.selectSignal(selectTotalBalance);

  ngOnInit(): void {
    this.store.dispatch(AccountsActions.loadAccounts());
  }

  reload(): void {
    this.store.dispatch(AccountsActions.loadAccounts());
  }
}
