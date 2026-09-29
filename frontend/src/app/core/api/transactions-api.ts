import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { ProcessedTransaction } from '../models/processed-transaction';
import { ProcessTransactionCommand, Transaction } from '../models/transaction';

@Injectable({ providedIn: 'root' })
export class TransactionsApi {
  private readonly http = inject(HttpClient);

  process(command: ProcessTransactionCommand): Observable<ProcessedTransaction> {
    return this.http
      .post<Transaction>('/api/transactions', command, { observe: 'response' })
      .pipe(
        map((response) => ({
          transaction: response.body as Transaction,
          alreadyProcessed: response.status === 200,
        })),
      );
  }
}
