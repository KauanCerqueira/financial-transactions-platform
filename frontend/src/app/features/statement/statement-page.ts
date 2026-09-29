import { Component, OnInit, computed, effect, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButton } from '@angular/material/button';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { Store } from '@ngrx/store';
import { StatementFilters } from '../../core/models/statement-filters';
import { TransactionType } from '../../core/models/transaction';
import { CurrencyBrlPipe } from '../../shared/pipes/currency-brl.pipe';
import { DateBrPipe } from '../../shared/pipes/date-br.pipe';
import { DateTimeBrPipe } from '../../shared/pipes/date-time-br.pipe';
import { ShortIdPipe } from '../../shared/pipes/short-id.pipe';
import { Icon } from '../../shared/ui/icon/icon';
import * as AccountsActions from '../accounts/state/accounts.actions';
import { selectAccounts } from '../accounts/state/accounts.selectors';
import { selectSubmissionResult } from '../transactions/state/transactions.selectors';
import * as StatementActions from './state/statement.actions';
import {
  selectStatementError,
  selectStatementItems,
  selectStatementPage,
  selectStatementPageSize,
  selectStatementStatus,
  selectStatementTotalItems,
} from './state/statement.selectors';

type StatementTab = 'statement' | 'info';
type TypeFilter = TransactionType | 'ALL';

@Component({
  selector: 'app-statement-page',
  imports: [RouterLink, MatButton, MatPaginator, CurrencyBrlPipe, DateBrPipe, DateTimeBrPipe, ShortIdPipe, Icon],
  templateUrl: './statement-page.html',
})
export class StatementPage implements OnInit {
  private readonly store = inject(Store);

  readonly accountId = input.required<string>();
  readonly standalone = input(false);

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

  readonly activeTab = signal<StatementTab>('statement');
  readonly filterType = signal<TypeFilter>('ALL');
  readonly filterFrom = signal('');
  readonly filterTo = signal('');
  readonly hasFilters = computed(
    () => this.filterType() !== 'ALL' || this.filterFrom() !== '' || this.filterTo() !== '',
  );

  private readonly submissionResult = this.store.selectSignal(selectSubmissionResult);
  private readonly recordedTransactionId = computed(() => this.submissionResult()?.transaction?.id);

  constructor() {
    effect(() => {
      this.recordedTransactionId();
      this.load(1, this.pageSize());
    });
  }

  ngOnInit(): void {
    this.store.dispatch(AccountsActions.loadAccounts());
  }

  onPage(event: PageEvent): void {
    this.load(event.pageIndex + 1, event.pageSize);
  }

  onTypeChange(value: string): void {
    this.filterType.set(value as TypeFilter);
    this.load(1, this.pageSize());
  }

  onFromChange(value: string): void {
    this.filterFrom.set(value);
    this.load(1, this.pageSize());
  }

  onToChange(value: string): void {
    this.filterTo.set(value);
    this.load(1, this.pageSize());
  }

  clearFilters(): void {
    this.filterType.set('ALL');
    this.filterFrom.set('');
    this.filterTo.set('');
    this.load(1, this.pageSize());
  }

  reload(): void {
    this.load(this.page(), this.pageSize());
  }

  private load(page: number, pageSize: number): void {
    this.store.dispatch(
      StatementActions.loadStatement({
        accountId: this.accountId(),
        page,
        pageSize,
        filters: this.currentFilters(),
      }),
    );
  }

  private currentFilters(): StatementFilters {
    const type = this.filterType();
    const from = this.filterFrom();
    const to = this.filterTo();

    return {
      type: type === 'ALL' ? null : type,
      from: from ? new Date(`${from}T00:00:00`).toISOString() : null,
      to: to ? new Date(`${to}T23:59:59`).toISOString() : null,
    };
  }
}
