import { ApplicationConfig, isDevMode, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { MatPaginatorIntl } from '@angular/material/paginator';
import { provideEffects } from '@ngrx/effects';
import { provideState, provideStore } from '@ngrx/store';
import { provideStoreDevtools } from '@ngrx/store-devtools';
import { authInterceptor, provideAuth, withAppInitializerAuthCheck } from 'angular-auth-oidc-client';
import { apiErrorInterceptor } from './core/interceptors/api-error.interceptor';
import { PtBrPaginatorIntl } from './core/i18n/pt-br-paginator';
import { AccountsEffects } from './features/accounts/state/accounts.effects';
import { accountsFeatureKey, accountsReducer } from './features/accounts/state/accounts.reducer';
import { StatementEffects } from './features/statement/state/statement.effects';
import { statementFeatureKey, statementReducer } from './features/statement/state/statement.reducer';
import { TransactionsEffects } from './features/transactions/state/transactions.effects';
import { transactionsFeatureKey, transactionsReducer } from './features/transactions/state/transactions.reducer';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(withInterceptors([authInterceptor(), apiErrorInterceptor])),

    provideAuth(
      {
        config: {
          authority: 'http://localhost:8081/realms/fraga',
          redirectUrl: window.location.origin,
          postLogoutRedirectUri: window.location.origin,
          clientId: 'financial-app',
          scope: 'openid profile email',
          responseType: 'code',
          silentRenew: true,
          useRefreshToken: true,
          secureRoutes: ['/api'],
        },
      },
      withAppInitializerAuthCheck(),
    ),

    provideStore(),
    provideState(accountsFeatureKey, accountsReducer),
    provideState(statementFeatureKey, statementReducer),
    provideState(transactionsFeatureKey, transactionsReducer),
    provideEffects(AccountsEffects, StatementEffects, TransactionsEffects),
    provideStoreDevtools({ maxAge: 25, logOnly: !isDevMode() }),

    { provide: MatPaginatorIntl, useClass: PtBrPaginatorIntl },
  ],
};
