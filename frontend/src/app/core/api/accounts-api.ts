import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Account } from '../models/account';
import { AccountsSummary } from '../models/accounts-summary';
import { PagedResult } from '../models/paged-result';
import { StatementFilters } from '../models/statement-filters';
import { Transaction } from '../models/transaction';

@Injectable({ providedIn: 'root' })
export class AccountsApi {
  private readonly http = inject(HttpClient);

  getAll(): Observable<Account[]> {
    return this.http.get<Account[]>('/api/accounts');
  }

  getSummary(): Observable<AccountsSummary> {
    return this.http.get<AccountsSummary>('/api/accounts/summary');
  }

  getStatement(
    accountId: string,
    page: number,
    pageSize: number,
    filters: StatementFilters = {},
  ): Observable<PagedResult<Transaction>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);

    if (filters.type) {
      params = params.set('type', filters.type);
    }

    if (filters.from) {
      params = params.set('from', filters.from);
    }

    if (filters.to) {
      params = params.set('to', filters.to);
    }

    return this.http.get<PagedResult<Transaction>>(`/api/accounts/${accountId}/transactions`, { params });
  }
}
