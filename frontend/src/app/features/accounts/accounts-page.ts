import { Component, OnInit, computed, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButton } from '@angular/material/button';
import { Store } from '@ngrx/store';
import { CurrencyBrlPipe } from '../../shared/pipes/currency-brl.pipe';
import { ShortIdPipe } from '../../shared/pipes/short-id.pipe';
import { Icon } from '../../shared/ui/icon/icon';
import * as AccountsActions from './state/accounts.actions';
import {
  selectAccounts,
  selectAccountsError,
  selectAccountsLoading,
  selectAccountsStatus,
  selectAccountsSummary,
  selectTotalBalance,
} from './state/accounts.selectors';

@Component({
  selector: 'app-accounts-page',
  imports: [RouterLink, MatButton, CurrencyBrlPipe, ShortIdPipe, Icon],
  templateUrl: './accounts-page.html',
})
export class AccountsPage implements OnInit {
  private readonly store = inject(Store);

  readonly accounts = this.store.selectSignal(selectAccounts);
  readonly status = this.store.selectSignal(selectAccountsStatus);
  readonly error = this.store.selectSignal(selectAccountsError);
  readonly loading = this.store.selectSignal(selectAccountsLoading);
  readonly totalBalance = this.store.selectSignal(selectTotalBalance);
  readonly summary = this.store.selectSignal(selectAccountsSummary);
  readonly searchText = input('');
  readonly selectedAccountId = input('');
  readonly hasTransactionPanel = input(false);
  readonly filteredAccounts = computed(() => {
    const query = (this.searchText() ?? '').trim().toLocaleLowerCase('pt-BR');
    return this.accounts().filter((account) =>
      `${account.holderName} ${account.id}`.toLocaleLowerCase('pt-BR').includes(query),
    );
  });

  ngOnInit(): void {
    this.store.dispatch(AccountsActions.loadAccounts());
  }

  reload(): void {
    this.store.dispatch(AccountsActions.loadAccounts());
  }
}
