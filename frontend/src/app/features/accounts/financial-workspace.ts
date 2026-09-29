import { Component, effect, inject, input } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Store } from '@ngrx/store';
import { AccountsPage } from './accounts-page';
import { NewAccountPage } from './new-account-page';
import * as AccountsActions from './state/accounts.actions';
import { selectSelectedAccountId } from './state/accounts.selectors';
import { StatementPage } from '../statement/statement-page';
import { NewTransactionPage } from '../transactions/new-transaction-page';

@Component({
  selector: 'app-financial-workspace',
  imports: [AccountsPage, StatementPage, NewTransactionPage, NewAccountPage],
  template: `
    <div class="workspace" [class.workspace--with-panel]="hasPanel">
      <div class="workspace__main">
        <app-accounts-page
          [searchText]="q()"
          [selectedAccountId]="selectedAccountId()"
          [hasTransactionPanel]="hasTransactionPanel"
          (accountSelected)="selectAccount($event)"
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
      @if (hasAccountPanel) {
        <aside class="transaction-panel" aria-label="Nova conta">
          <app-new-account-page />
        </aside>
      }
    </div>
  `,
})
export class FinancialWorkspace {
  private readonly store = inject(Store);
  private readonly route = inject(ActivatedRoute);
  private readonly path = this.route.snapshot.routeConfig?.path ?? '';

  readonly accountId = input('');
  readonly q = input('');
  readonly hasTransactionPanel = this.path === 'transactions/new';
  readonly hasAccountPanel = this.path === 'accounts/new';
  readonly hasPanel = this.hasTransactionPanel || this.hasAccountPanel;
  readonly selectedAccountId = this.store.selectSignal(selectSelectedAccountId);

  constructor() {
    effect(() => {
      // links que chegam com ?accountId= (ex.: lançar para uma conta específica)
      const requested = this.accountId();

      if (requested) {
        this.selectAccount(requested);
      }
    });
  }

  selectAccount(accountId: string): void {
    this.store.dispatch(AccountsActions.selectAccount({ accountId }));
  }
}
