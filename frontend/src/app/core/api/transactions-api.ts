import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ProcessTransactionCommand } from '../models/transaction';
import { TransactionAccepted } from '../models/transaction-event';

@Injectable({ providedIn: 'root' })
export class TransactionsApi {
  private readonly http = inject(HttpClient);

  enqueue(command: ProcessTransactionCommand): Observable<TransactionAccepted> {
    return this.http.post<TransactionAccepted>('/api/transactions', command);
  }

  getStatus(eventId: string): Observable<TransactionAccepted> {
    return this.http.get<TransactionAccepted>(`/api/transactions/${eventId}`);
  }
}
