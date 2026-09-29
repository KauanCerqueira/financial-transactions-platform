import { Routes } from '@angular/router';
import { autoLoginPartialRoutesGuard } from 'angular-auth-oidc-client';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'accounts' },
  {
    path: 'accounts',
    title: 'Contas · FRAGA Financeiro',
    canActivate: [autoLoginPartialRoutesGuard],
    loadComponent: () => import('./features/accounts/accounts-page').then((module) => module.AccountsPage),
  },
  {
    path: 'accounts/:accountId/statement',
    title: 'Extrato · FRAGA Financeiro',
    canActivate: [autoLoginPartialRoutesGuard],
    loadComponent: () => import('./features/statement/statement-page').then((module) => module.StatementPage),
  },
  {
    path: 'transactions/new',
    title: 'Lançar transação · FRAGA Financeiro',
    canActivate: [autoLoginPartialRoutesGuard],
    loadComponent: () =>
      import('./features/transactions/new-transaction-page').then((module) => module.NewTransactionPage),
  },
  { path: '**', redirectTo: 'accounts' },
];
