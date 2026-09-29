import { Component, effect, inject, input, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Store } from '@ngrx/store';
import { AccountsPage } from './accounts-page';
import { selectAccounts } from './state/accounts.selectors';
import { StatementPage } from '../statement/statement-page';
import { NewTransactionPage } from '../transactions/new-transaction-page';

@Component({
  selector: 'app-financial-workspace',
  imports: [AccountsPage, StatementPage, NewTransactionPage],
  template: `
    <div class="workspace" [class.workspace--with-panel]="hasTransactionPanel">
      <div class="workspace__main">
        <app-accounts-page
          [searchText]="q()"
          [selectedAccountId]="selectedAccountId()"
          [hasTransactionPanel]="hasTransactionPanel"
          (accountSelected)="selectedAccountId.set($event)"
        />
        @if (selectedAccountId(); as selected) {
          <app-statement-page [accountId]="selected" />
        }
      </div>
      @if (hasTransactionPanel) {
        <aside class="transaction-panel" aria-label="Nova transação">
          <app-new-transaction-page [initialAccountId]="selectedAccountId()" />
        </aside>
      }
    </div>
  `,
})
export class FinancialWorkspace {
  private readonly store = inject(Store);
  private readonly route = inject(ActivatedRoute);
  private readonly accounts = this.store.selectSignal(selectAccounts);

  readonly accountId = input('');
  readonly q = input('');
  readonly hasTransactionPanel = this.route.snapshot.routeConfig?.path === 'transactions/new';
  readonly selectedAccountId = signal('');

  constructor() {
    effect(() => {
      const accounts = this.accounts();
      const current = this.selectedAccountId();

      if (accounts.length === 0 || accounts.some((account) => account.id === current)) {
        return;
      }

      const requested = this.accountId();
      const initial = accounts.some((account) => account.id === requested) ? requested : accounts[0].id;

      this.selectedAccountId.set(initial);
    });
  }
}
