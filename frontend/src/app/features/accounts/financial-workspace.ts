import { Component, computed, inject, input } from '@angular/core';
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
  readonly selectedAccountId = computed(() => this.accountId() || this.accounts()[0]?.id || '');
}
