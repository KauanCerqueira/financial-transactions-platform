import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ProcessTransactionCommand, Transaction } from '../models/transaction';

@Injectable({ providedIn: 'root' })
export class TransactionsApi {
  private readonly http = inject(HttpClient);

  process(command: ProcessTransactionCommand): Observable<Transaction> {
    return this.http.post<Transaction>('/api/transactions', command);
  }
}
