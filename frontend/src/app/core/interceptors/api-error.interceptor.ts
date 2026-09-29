import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { catchError, throwError } from 'rxjs';
import { ApiError } from '../models/api-error';
import { messageForCode } from '../models/api-messages';

export const apiErrorInterceptor: HttpInterceptorFn = (request, next) => {
  const oidcSecurityService = inject(OidcSecurityService);

  return next(request).pipe(
    catchError((response: HttpErrorResponse) => {
      if (response.status === 401) {
        oidcSecurityService.logoffLocal();
        oidcSecurityService.authorize();
      }

      return throwError(() => toApiError(response));
    }),
  );
};

function toApiError(response: HttpErrorResponse): ApiError {
  if (response.status === 0) {
    return new ApiError('NETWORK_ERROR', messageForCode('NETWORK_ERROR', 'Falha de rede.'), 0);
  }

  const body = response.error as { code?: string; detail?: string } | null;
  const code = body?.code ?? `HTTP_${response.status}`;

  return new ApiError(code, messageForCode(code, fallbackMessage(response, body)), response.status);
}

function fallbackMessage(response: HttpErrorResponse, body: { detail?: string } | null): string {
  if (response.status === 401) {
    return 'Sua sessão expirou. Entrando novamente...';
  }

  if (response.status >= 500) {
    return 'Ocorreu um erro inesperado. Tente novamente em instantes.';
  }

  return body?.detail ?? 'Não foi possível concluir a operação.';
}
