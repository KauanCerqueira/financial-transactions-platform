import { Component, OnInit, computed, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButton } from '@angular/material/button';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { Store } from '@ngrx/store';
import { CountUpDirective } from '../../shared/directives/count-up.directive';
import { CurrencyBrlPipe } from '../../shared/pipes/currency-brl.pipe';
import { DateTimeBrPipe } from '../../shared/pipes/date-time-br.pipe';
import { Icon } from '../../shared/ui/icon/icon';
import * as AccountsActions from '../accounts/state/accounts.actions';
import { selectAccounts } from '../accounts/state/accounts.selectors';
import * as StatementActions from './state/statement.actions';
import {
  selectStatementError,
  selectStatementItems,
  selectStatementPage,
  selectStatementPageSize,
  selectStatementStatus,
  selectStatementTotalItems,
} from './state/statement.selectors';

@Component({
  selector: 'app-statement-page',
  imports: [RouterLink, MatButton, MatPaginator, CurrencyBrlPipe, DateTimeBrPipe, CountUpDirective, Icon],
  templateUrl: './statement-page.html',
})
export class StatementPage implements OnInit {
  private readonly store = inject(Store);

  readonly accountId = input.required<string>();

  private readonly accounts = this.store.selectSignal(selectAccounts);
  readonly account = computed(
    () => this.accounts().find((candidate) => candidate.id === this.accountId()) ?? null,
  );

  readonly items = this.store.selectSignal(selectStatementItems);
  readonly page = this.store.selectSignal(selectStatementPage);
  readonly pageSize = this.store.selectSignal(selectStatementPageSize);
  readonly totalItems = this.store.selectSignal(selectStatementTotalItems);
  readonly status = this.store.selectSignal(selectStatementStatus);
  readonly error = this.store.selectSignal(selectStatementError);

  ngOnInit(): void {
    this.store.dispatch(AccountsActions.loadAccounts());
    this.load(1, 10);
  }

  onPage(event: PageEvent): void {
    this.load(event.pageIndex + 1, event.pageSize);
  }

  reload(): void {
    this.load(this.page(), this.pageSize());
  }

  private load(page: number, pageSize: number): void {
    this.store.dispatch(StatementActions.loadStatement({ accountId: this.accountId(), page, pageSize }));
  }
}
