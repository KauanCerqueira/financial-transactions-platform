import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { ApiError } from '../models/api-error';

const messagesByCode: Record<string, string> = {
  INSUFFICIENT_FUNDS: 'Saldo insuficiente para esta movimentação.',
  DUPLICATE_EVENT: 'Este evento já foi processado.',
  ACCOUNT_NOT_FOUND: 'Conta não encontrada.',
};

export const apiErrorInterceptor: HttpInterceptorFn = (request, next) =>
  next(request).pipe(catchError((response: HttpErrorResponse) => throwError(() => toApiError(response))));

function toApiError(response: HttpErrorResponse): ApiError {
  if (response.status === 0) {
    return new ApiError('NETWORK_ERROR', 'Não foi possível falar com o servidor. Verifique a conexão.', 0);
  }

  const body = response.error as { code?: string; detail?: string } | null;
  const code = body?.code ?? `HTTP_${response.status}`;

  return new ApiError(code, messagesByCode[code] ?? fallbackMessage(response, body), response.status);
}

function fallbackMessage(response: HttpErrorResponse, body: { detail?: string } | null): string {
  if (response.status >= 500) {
    return 'Ocorreu um erro inesperado. Tente novamente em instantes.';
  }

  return body?.detail ?? 'Não foi possível concluir a operação.';
}
