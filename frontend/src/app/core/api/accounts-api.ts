import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Account } from '../models/account';
import { PagedResult } from '../models/paged-result';
import { Transaction } from '../models/transaction';

@Injectable({ providedIn: 'root' })
export class AccountsApi {
  private readonly http = inject(HttpClient);

  getAll(): Observable<Account[]> {
    return this.http.get<Account[]>('/api/accounts');
  }

  getStatement(accountId: string, page: number, pageSize: number): Observable<PagedResult<Transaction>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);

    return this.http.get<PagedResult<Transaction>>(`/api/accounts/${accountId}/transactions`, { params });
  }
}
